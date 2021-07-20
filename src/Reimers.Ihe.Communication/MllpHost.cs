// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MllpHost.cs" company="Reimers.dk">
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
    using System.Buffers;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net.Security;
    using System.Net.Sockets;
    using System.Security.Authentication;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using NHapi.Base.Parser;

    internal class MllpHost : IAsyncDisposable
    {
        private readonly TcpClient _client;
        private readonly IMessageLog _messageLog;
        private readonly PipeParser _parser;
        private readonly Encoding _encoding;
        private readonly IHl7MessageMiddleware _middleware;
        private readonly int _bufferSize;
        private readonly CancellationTokenSource _tokenSource = new();
        private readonly SemaphoreSlim _asyncLock = new(1, 1);
        private Stream _stream = null!;
        private Task _readThread = null!;

        private MllpHost(
            TcpClient client,
            IMessageLog messageLog,
            PipeParser parser,
            Encoding encoding,
            IHl7MessageMiddleware middleware,
            int bufferSize)
        {
            _client = client;
            _messageLog = messageLog;
            _parser = parser;
            _encoding = encoding;
            _middleware = middleware;
            _bufferSize = bufferSize;
        }

        public bool IsConnected => _client.Connected;

        public static async Task<MllpHost> Create(
            TcpClient tcpClient,
            IMessageLog messageLog,
            IHl7MessageMiddleware middleware,
            PipeParser? parser = null,
            Encoding? encoding = null,
            X509Certificate? serverCertificate = null,
            RemoteCertificateValidationCallback?
                userCertificateValidationCallback = null,
            int bufferSize = 4096)
        {
            var host = new MllpHost(
                tcpClient,
                messageLog,
                parser ?? new PipeParser(),
                encoding ?? Encoding.ASCII,
                middleware,
                bufferSize);
            Stream stream = tcpClient.GetStream();

            if (serverCertificate != null)
            {
                var ssl = new SslStream(
                    stream,
                    false,
                    userCertificateValidationCallback);
                await ssl.AuthenticateAsServerAsync(
                        serverCertificate,
                        true,
                        SslProtocols.Tls11 | SslProtocols.Tls12,
                        false)
                    .ConfigureAwait(false);
                host._stream = ssl;
            }
            else
            {
                host._stream = stream;
            }

            host._readThread = host.ReadStream(host._tokenSource.Token);
            return host;
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            _tokenSource.Cancel();
            await _readThread.ConfigureAwait(false);
            _tokenSource.Dispose();
            _stream.Close();
            await _stream.DisposeAsync().ConfigureAwait(false);
            _client.Close();
            _client.Dispose();
            GC.SuppressFinalize(this);
        }

        private async Task ReadStream(CancellationToken cancellationToken)
        {
            await Task.Yield();
            try
            {
                var messageBuilder = new List<byte>();
                while (!cancellationToken.IsCancellationRequested)
                {
                    var index = 0;
                    var buffer = ArrayPool<byte>.Shared.Rent(_bufferSize);
                    var read = await _stream.ReadAsync(buffer.AsMemory(0, _bufferSize), cancellationToken)
                        .ConfigureAwait(false);
                    while (index > -1)
                    {
                        index = Process(
                            buffer,
                            index,
                            messageBuilder,
                            read - index,
                            cancellationToken);
                    }
                    ArrayPool<byte>.Shared.Return(buffer);
                }
            }
            catch (TaskCanceledException) { }
            catch (IOException io)
            {
                var msg = io.Message;
                Trace.TraceInformation(msg);
            }
            finally
            {
                _client.Close();
                _client.Dispose();
            }
        }

        private int Process(byte[] buffer, int index, List<byte> messageBuilder, int read, CancellationToken cancellationToken)
        {
            if (read < 0)
            {
                return -1;
            }
            var isStart = buffer[index] == 11;
            if (isStart)
            {
                if (messageBuilder.Count > 0)
                {
                    throw new Exception(
                        "Unexpected character: "
                        + buffer[index].ToString("x2"));
                }
            }

            var endblockStart = Array.IndexOf(
                buffer,
                Constants.EndBlock[0],
                index,
                read);
            var endblockEnd = endblockStart + 1;
            var bytes = buffer.Skip(index).Take(endblockStart > -1 ? endblockStart - index : read);
            if (isStart)
            {
                bytes = bytes.Skip(1);
            }

            if (buffer[endblockEnd] == Constants.EndBlock[1])
            {
                messageBuilder.AddRange(bytes);
                _ = SendResponse(messageBuilder.ToArray(), cancellationToken)
                    .ConfigureAwait(false);
                messageBuilder.Clear();
            }
            else
            {
                messageBuilder.AddRange(bytes);
            }

            return endblockStart == -1 ? -1 : Array.IndexOf(buffer, Constants.StartBlock[0], endblockEnd);
        }

        private async Task SendResponse(
            byte[] messageBuilder,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var s = _encoding.GetString(messageBuilder);
            var received = _parser.Parse(s);
            var message = new Hl7Message(
                received,
                _client.Client.RemoteEndPoint?.ToString() ?? string.Empty);
            await _messageLog.Write(s).ConfigureAwait(false);
            var result = await _middleware.Handle(message, cancellationToken)
                .ConfigureAwait(false);

            cancellationToken.ThrowIfCancellationRequested();
            string resultMsg = _parser.Encode(result);
            await WriteToStream(resultMsg, cancellationToken)
                .ConfigureAwait(false);
            await _messageLog.Write(resultMsg).ConfigureAwait(false);
        }

        private async Task WriteToStream(
            string response,
            CancellationToken cancellationToken)
        {
            await _asyncLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            await _messageLog.Write(response).ConfigureAwait(false);
            var bytes = _encoding.GetBytes(response);
            var count = bytes.Length + 3;
            var buffer = ArrayPool<byte>.Shared.Rent(count);
            Constants.StartBlock.CopyTo(buffer, 0);
            bytes.CopyTo(buffer, 1);
            Constants.EndBlock.CopyTo(buffer, bytes.Length + 1);

            await _stream.WriteAsync(
                    buffer.AsMemory(
                    0,
                    count),
                    cancellationToken)
                .ConfigureAwait(false);

            ArrayPool<byte>.Shared.Return(buffer);
            _asyncLock.Release(1);
        }
    }
}
