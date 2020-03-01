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

    internal class MllpHost : IDisposable
    {
        private readonly TcpClient _client;
        private readonly IMessageLog _messageLog;
        private readonly Encoding _encoding;
        private readonly IHl7MessageMiddleware _middleware;

        private readonly CancellationTokenSource _tokenSource =
            new CancellationTokenSource();

        private Stream _stream;
        private Task _readThread;

        private MllpHost(
            TcpClient client,
            IMessageLog messageLog,
            Encoding encoding,
            IHl7MessageMiddleware middleware)
        {
            _client = client;
            _messageLog = messageLog;
            _encoding = encoding;
            _middleware = middleware;
        }

        public bool IsConnected => _client.Connected;

        public static async Task<MllpHost> Create(
            TcpClient tcpClient,
            IMessageLog messageLog,
            IHl7MessageMiddleware middleware,
            Encoding encoding = null,
            X509Certificate serverCertificate = null,
            RemoteCertificateValidationCallback
                userCertificateValidationCallback = null)
        {
            var host = new MllpHost(
                tcpClient,
                messageLog,
                encoding ?? Encoding.ASCII,
                middleware);
            var stream = tcpClient.GetStream();
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

        public void Dispose()
        {
            _client.Close();
            _client.Dispose();
            _stream?.Close();
            _stream?.Dispose();
        }

        private async Task ReadStream(CancellationToken cancellationToken)
        {
            await Task.Yield();
            try
            {
                byte previous = 0;
                var messageBuilder = new List<byte>();
                while (true)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var current = (byte)_stream.ReadByte();
                    if (Constants.EndBlock.SequenceEqual(
                        new[] { previous, current }))
                    {
                        messageBuilder.RemoveAt(messageBuilder.Count - 1);
                        await SendResponse(
                                messageBuilder.ToArray(),
                                cancellationToken)
                            .ConfigureAwait(false);
                        break;
                    }

                    if (previous == 0 && current == 11)
                    {
                        if (messageBuilder.Count > 0)
                        {
                            throw new Exception(
                                "Unexpected character: "
                                + current.ToString("x2"));
                        } // Start char received.
                    }
                    else
                    {
                        messageBuilder.Add(current);
                        previous = current;
                    }
                }
            }
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

        private async Task SendResponse(
            byte[] messageBuilder,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var s = _encoding.GetString(messageBuilder);
            var message = new Hl7Message(
                s,
                _client.Client.RemoteEndPoint.ToString());
            await _messageLog.Write(s).ConfigureAwait(false);
            var result = await _middleware.Handle(message, cancellationToken)
                .ConfigureAwait(false);

            cancellationToken.ThrowIfCancellationRequested();
            await WriteToStream(result, cancellationToken)
                .ConfigureAwait(false);
        }

        private async Task WriteToStream(
            string response,
            CancellationToken cancellationToken)
        {
            var bytes = _encoding.GetBytes(response);
            var buffer = ArrayPool<byte>.Shared.Rent(bytes.Length + 3);
            Constants.StartBlock.CopyTo(buffer, 0);
            bytes.CopyTo(buffer, 1);
            Constants.EndBlock.CopyTo(buffer, bytes.Length + 1);

            await _stream.WriteAsync(
                    buffer,
                    0,
                    bytes.Length + 3,
                    cancellationToken)
                .ConfigureAwait(false);

            ArrayPool<byte>.Shared.Return(buffer);
        }
    }
}
