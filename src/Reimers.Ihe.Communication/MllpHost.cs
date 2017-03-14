namespace Reimers.Ihe.Communication
{
    using System;
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
    using System.Threading.Tasks;    internal class MllpHost : IDisposable
    {
        private readonly TcpClient _client;
        private readonly Encoding _encoding;
        private readonly IHl7MessageMiddleware _middleware;
        private readonly CancellationTokenSource _tokenSource = new CancellationTokenSource();
        private Stream _stream;
        private Task _readThread;        private MllpHost(TcpClient client, Encoding encoding, IHl7MessageMiddleware middleware)
        {
            _client = client;
            _encoding = encoding;
            _middleware = middleware;
        }        public bool IsConnected => _client.Connected;        public static async Task<MllpHost> Create(TcpClient tcpClient, IHl7MessageMiddleware middleware, Encoding encoding = null, X509Certificate serverCertificate = null)
        {
            var host = new MllpHost(tcpClient, encoding ?? Encoding.ASCII, middleware);
            var stream = tcpClient.GetStream();
            if (serverCertificate != null)
            {
                var ssl = new SslStream(stream, false);                await ssl.AuthenticateAsServerAsync(serverCertificate, true, SslProtocols.Default | SslProtocols.Tls12, false).ConfigureAwait(false);
                host._stream = ssl;
            }
            else
            {
                host._stream = stream;
            }
            host._readThread = host.ReadStream(host._tokenSource.Token);            return host;
        }        public void Dispose()
        {
            _client.Close();
            _client.Dispose();
            _stream?.Close();
            _stream?.Dispose();
        }        private async Task ReadStream(CancellationToken cancellationToken)
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
                    if (Constants.EndBlock.SequenceEqual(new[] { previous, current }))
                    {
                        messageBuilder.RemoveAt(messageBuilder.Count - 1);
                        await SendResponse(messageBuilder.ToArray(), cancellationToken).ConfigureAwait(false);
                        break;
                    }                    if (previous == 0 && current == 11)
                    {
                        if (messageBuilder.Count > 0)
                        {
                            throw new Exception(
                                $"Unexpected character: {current:x2}");
                        }                        // Start char received.
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
        }        private async Task SendResponse(
            byte[] messageBuilder, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var s = _encoding.GetString(messageBuilder);            var message = new Hl7Message(
                s,
                _client.Client.RemoteEndPoint.ToString());
            var result = await _middleware.Handle(message);            await WriteToStream(result, cancellationToken);
        }        private async Task WriteToStream(string response, CancellationToken cancellationToken)
        {
            var buffer =
                Constants.StartBlock
                    .Concat(_encoding.GetBytes(response))
                    .Concat(Constants.EndBlock)
                    .ToArray();
            await _stream.WriteAsync(buffer, 0, buffer.Length, cancellationToken);
        }
    }
}