// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MllpClient.cs" company="Reimers.dk">
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
    using NHapi.Base.Model;
    using NHapi.Base.Parser;

    /// <summary>
    /// Defines the <see cref="MllpClient"/> class.
    /// </summary>
    public class MllpClient : IHostConnection
    {
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private readonly Dictionary<string, TaskCompletionSource<Hl7Message>> _messages = new Dictionary<string, TaskCompletionSource<Hl7Message>>();
        private readonly string _address;
        private readonly int _port;
        private readonly IMessageLog _messageLog;
        private readonly PipeParser _parser;
        private readonly Encoding _encoding;
        private readonly X509CertificateCollection? _clientCertificates;
        private readonly RemoteCertificateValidationCallback? _userCertificateValidationCallback;
        private string _remoteAddress = null!;
        private Stream _stream = null!;
        private readonly CancellationTokenSource _tokenSource = new CancellationTokenSource();
        private TcpClient _tcpClient = null!;
#pragma warning disable IDE0052 // Remove unread private members
        private Task _readThread = null!;
#pragma warning restore IDE0052 // Remove unread private members

        private MllpClient(
            string address,
            int port,
            IMessageLog messageLog,
            PipeParser parser,
            Encoding encoding,
            X509CertificateCollection? clientCertificates,
            RemoteCertificateValidationCallback? userCertificateValidationCallback)
        {
            _address = address;
            _port = port;
            _messageLog = messageLog;
            _parser = parser;
            _encoding = encoding;
            _clientCertificates = clientCertificates;
            _userCertificateValidationCallback =
                userCertificateValidationCallback;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="MllpClient"/> class.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="port"></param>
        /// <param name="messageLog"></param>
        /// <param name="parser"></param>
        /// <param name="encoding"></param>
        /// <param name="clientCertificates"></param>
        /// <param name="userCertificateValidationCallback"></param>
        /// <returns></returns>
        public static async Task<IHostConnection> Create(
            string address,
            int port,
            IMessageLog? messageLog = null,
            PipeParser? parser = null,
            Encoding? encoding = null,
            X509CertificateCollection? clientCertificates = null,
            RemoteCertificateValidationCallback?
                userCertificateValidationCallback = null)
        {
            var instance = new MllpClient(
                address,
                port,
                messageLog ?? NullLog.Get(),
                parser ?? new PipeParser(),
                encoding ?? Encoding.ASCII,
                clientCertificates,
                userCertificateValidationCallback);
            await instance.Setup().ConfigureAwait(false);
            return instance;
        }

        /// <summary>
        /// Sends the HL7 message to the server.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
        /// <returns></returns>
        public async Task<Hl7Message> Send<TMessage>(
            TMessage message,
            CancellationToken cancellationToken = default)
        where TMessage : IMessage
        {
            await _semaphore.WaitAsync(cancellationToken);

            var hl7 = _parser.Encode(message);
            var bytes = _encoding.GetBytes(hl7);
            var length = bytes.Length
                         + Constants.StartBlock.Length
                         + Constants.EndBlock.Length;
            var buffer = ArrayPool<byte>.Shared.Rent(length);
            Array.Copy(
                Constants.StartBlock,
                0,
                buffer,
                0,
                Constants.StartBlock.Length);
            Array.Copy(
                bytes,
                0,
                buffer,
                Constants.StartBlock.Length,
                bytes.Length);
            Array.Copy(
                Constants.EndBlock,
                0,
                buffer,
                Constants.StartBlock.Length + bytes.Length,
                Constants.EndBlock.Length);

            var completionSource = new TaskCompletionSource<Hl7Message>();
            var key = message.GetMessageControlId();
            _messages.Add(key, completionSource);
            await _messageLog.Write(hl7).ConfigureAwait(false);
            await _stream.WriteAsync(buffer, 0, length, cancellationToken)
                .ConfigureAwait(false);
            ArrayPool<byte>.Shared.Return(buffer);
            _semaphore.Release(1);
            return await completionSource.Task.ConfigureAwait(false);
        }

        private async Task Setup()
        {
            _tcpClient = new TcpClient(_address, _port);
            _remoteAddress = _tcpClient.Client.RemoteEndPoint.ToString();
            _stream = _tcpClient.GetStream();
            if (_clientCertificates != null)
            {
                var ssl = new SslStream(
                    _stream,
                    false,
                    _userCertificateValidationCallback);
                await ssl.AuthenticateAsClientAsync(
                        _address,
                        _clientCertificates,
                        SslProtocols.Tls11 | SslProtocols.Tls12,
                        false)
                    .ConfigureAwait(false);
                _stream = ssl;
            }

            _readThread = ReadStream(_tokenSource.Token);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _tcpClient.Close();
            _tcpClient.Dispose();
            _tokenSource.Cancel();
            _tokenSource.Dispose();
            _stream.Close();
            _stream.Dispose();
        }

        private async Task ReadStream(CancellationToken cancellationToken)
        {
            await Task.Yield();
            try
            {
                byte previous = 0;
                var messageBuilder = new List<byte>();
                while (!cancellationToken.IsCancellationRequested)
                {
                    var current = (byte)_stream.ReadByte();

                    cancellationToken.ThrowIfCancellationRequested();
                    if (Constants.EndBlock.SequenceEqual(
                        new[] { previous, current }))
                    {
                        messageBuilder.RemoveAt(messageBuilder.Count - 1);
                        var s = _encoding.GetString(messageBuilder.ToArray());
                        messageBuilder.Clear();
                        previous = 0;
                        var msg = _parser.Parse(s);
                        var message = new Hl7Message(msg, _remoteAddress);
                        var controlId = msg.GetMessageControlId();
                        var completionSource = _messages[controlId];
                        _messages.Remove(controlId);
                        completionSource.SetResult(message);
                        continue;
                    }

                    if (previous == 0 && current == 11)
                    {
                        if (messageBuilder.Count > 0)
                        {
                            foreach (var completionSource in _messages.Values)
                            {
                                completionSource.SetException(
                                   new Exception(
                                       $"Unexpected character: {current:x2}"));

                            }
                            _messages.Clear();
                        }
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
                Trace.TraceInformation(io.Message);
                if (!cancellationToken.IsCancellationRequested)
                {
                    foreach (var completionSource in _messages.Values)
                    {
                        completionSource.SetException(io);

                    }
                    _messages.Clear();
                }
            }
        }
    }
}
