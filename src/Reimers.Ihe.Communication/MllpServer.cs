// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MllpServer.cs" company="Reimers.dk">
//   Copyright © Reimers.dk 2017
//   This source is subject to the MIT License.
//   Please see https://opensource.org/licenses/MIT for details.
//   All other rights reserved.
//
//   The above copyright notice and this permission notice shall be included in
//   all copies or substantial portions of the Software.
//
//   THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//   IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//   FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//   AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//   LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//   OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//   THE SOFTWARE.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Reimers.Ihe.Communication
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Security;
    using System.Net.Sockets;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using NHapi.Base.Parser;

    /// <summary>
    /// Defines an IHE server using MLLP connections.
    /// </summary>
    public class MllpServer : IAsyncDisposable
    {
        private readonly IMessageLog _messageLog;
        private readonly IHl7MessageMiddleware _middleware;
        private readonly PipeParser? _parser;
        private readonly Encoding _encoding;
        private readonly X509Certificate? _serverCertificate;
        private readonly RemoteCertificateValidationCallback? _userCertificateValidationCallback;
        private readonly int _bufferSize;
        private readonly TcpListener _listener;
        private readonly List<MllpHost> _connections = new();
        private readonly Timer _timer;
        private readonly CancellationTokenSource _tokenSource = new();
        private Task? _readTask;

        /// <summary>
        /// Initializes a new instance of the <see cref="MllpServer"/> class.
        /// </summary>
        /// <param name="endPoint">The <see cref="IPEndPoint"/> the server will listen on.</param>
        /// <param name="messageLog">The <see cref="IMessageLog"/> to use for logging incoming messages.</param>
        /// <param name="middleware">The message handling middleware.</param>
        /// <param name="cleanupInterval">The interval between cleaning up client connections.</param>
        /// <param name="parser">The <see cref="PipeParser"/> to use for parsing and encoding.</param>
        /// <param name="encoding">The <see cref="Encoding"/> to use for network transfers.</param>
        /// <param name="serverCertificate">The certificates to use for secure connections.</param>
        /// <param name="userCertificateValidationCallback">Optional certificate validation callback.</param>
        /// <param name="bufferSize">Read buffer size.</param>
        public MllpServer(
            IPEndPoint endPoint,
            IMessageLog messageLog,
            IHl7MessageMiddleware middleware,
            TimeSpan cleanupInterval = default,
            PipeParser? parser = null,
            Encoding? encoding = null,
            X509Certificate? serverCertificate = null,
            RemoteCertificateValidationCallback?
                userCertificateValidationCallback = null,
            int bufferSize = 256)
        {
            _messageLog = messageLog;
            _middleware = middleware;
            _parser = parser;
            _encoding = encoding ?? Encoding.ASCII;
            _serverCertificate = serverCertificate;
            _userCertificateValidationCallback =
                userCertificateValidationCallback;
            _bufferSize = bufferSize;
            _listener = new TcpListener(endPoint);
            cleanupInterval = cleanupInterval == default
                ? TimeSpan.FromSeconds(5)
                : cleanupInterval;
            _timer = new Timer(
                async _ => await CleanConnections().ConfigureAwait(false),
                null,
                cleanupInterval,
                cleanupInterval);
        }

        /// <summary>
        /// Starts the server
        /// </summary>
        public void Start()
        {
            _listener.Start();
            _readTask = Read();
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            _timer.Change(TimeSpan.FromDays(1), TimeSpan.FromDays(1));
            _tokenSource.Cancel();
            _listener.Stop();
            await _timer.DisposeAsync().ConfigureAwait(false);
            if (_readTask != null && _readTask.Status != TaskStatus.WaitingForActivation)
            {
                await _readTask.ConfigureAwait(false);
            }

            _tokenSource.Dispose();

            await CleanConnections().ConfigureAwait(false);
            MllpHost[] hosts;
            lock (_connections)
            {
                hosts = _connections.ToArray();
            }

            async Task DisposeConnection(MllpHost connection)
            {
                try
                {
                    await connection.DisposeAsync().ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                }
            }

            await Task.WhenAll(hosts.Select(DisposeConnection)).ConfigureAwait(false);

            GC.SuppressFinalize(this);
        }

        private async Task Read()
        {
            while (!_tokenSource.IsCancellationRequested)
            {
                try
                {
                    var client = await _listener.AcceptTcpClientAsync()
                        .ConfigureAwait(false);
                    var connection = await MllpHost.Create(
                            client,
                            _messageLog,
                            _middleware,
                            _parser,
                            _encoding,
                            _serverCertificate,
                            _userCertificateValidationCallback,
                            _bufferSize)
                        .ConfigureAwait(false);
                    lock (_connections)
                    {
                        _connections.Add(connection);
                    }
                }
                catch (SocketException s) when (s.ErrorCode == 995)
                {
                    break;
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
            }
        }

        private async ValueTask CleanConnections()
        {
            MllpHost[] temp;
            lock (_connections)
            {
                temp = _connections.Where(x => !x.IsConnected).ToArray();
                foreach (var conn in temp)
                {
                    _connections.Remove(conn);
                }
            }

            var disposeTasks = temp.Select(
                async host =>
                {
                    try
                    {
                        await host.DisposeAsync().ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                    }
                });
            await Task.WhenAll(disposeTasks).ConfigureAwait(false);
        }
    }
}
