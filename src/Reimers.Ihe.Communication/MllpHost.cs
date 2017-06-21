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
    using System.Threading.Tasks;

    internal class MllpHost : IDisposable
    {
        private readonly TcpClient _client;
        private readonly Encoding _encoding;
        private readonly IHl7MessageMiddleware _middleware;
        private readonly CancellationToken _token;
        private readonly Stream _stream;

        private MllpHost(TcpClient client, Encoding encoding, IHl7MessageMiddleware middleware, Stream stream, CancellationToken cancellationToken)
        {
            _client = client;
            _encoding = encoding;
            _middleware = middleware;
            _stream = stream;
            _token = cancellationToken;
        }

        public bool IsConnected => _client.Connected;

        public static async Task<MllpHost> Create(TcpClient tcpClient, IHl7MessageMiddleware middleware, Encoding encoding = null, ServerSecurityDetails securityDetails = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Stream stream;
            NetworkStream networkStream = tcpClient.GetStream();

            if (securityDetails != null)
            {
                var sslStream = new SslStream(networkStream, true, securityDetails.ClientCertificateValidationCallback, null);

                try
                {
                    bool askForClientCertificate = securityDetails.ForceClientAuthentciation;
                    await sslStream.AuthenticateAsServerAsync(securityDetails.ServerCertificate, askForClientCertificate, securityDetails.SupportedSslProtocols, false);

                    if (askForClientCertificate && !sslStream.IsMutuallyAuthenticated)
                    {
                        throw new AuthenticationException("mutual authentication failed.");
                    }
                }
                catch (Exception)
                {
                    sslStream.Dispose();
                    throw;
                }

                stream = sslStream;
            }
            else
            {
                stream = networkStream;
            }

            var host = new MllpHost(tcpClient, encoding ?? Encoding.ASCII, middleware, stream, cancellationToken);
            host.ReadStream(host._token);
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
                    if (Constants.EndBlock.SequenceEqual(new[] { previous, current }))
                    {
                        messageBuilder.RemoveAt(messageBuilder.Count - 1);
                        await this.SendResponse(messageBuilder.ToArray(), cancellationToken);
                        break;
                    }
                    if (previous == 0 && current == 11)
                    {
                        if (messageBuilder.Count > 0)
                        {
                            throw new Exception($"Unexpected character: {current:x2}");
                        } // Start char received. 
                    }
                    else
                    {
                        messageBuilder.Add(current);
                        previous = current;
                    }
                }
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.Message);
            }
            finally
            {
                _client.Dispose();
            }
        }

        private async Task SendResponse(byte[] messageBuilder, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var s = _encoding.GetString(messageBuilder);
            var message = new Hl7Message(s, _client.Client.RemoteEndPoint.ToString());
            var result = await _middleware.Handle(message).ConfigureAwait(false);
            await WriteToStream(result, cancellationToken).ConfigureAwait(false);
        }

        private async Task WriteToStream(string response, CancellationToken cancellationToken)
        {
            var buffer =
                Constants.StartBlock
                    .Concat(_encoding.GetBytes(response))
                    .Concat(Constants.EndBlock)
                    .ToArray();
            await _stream.WriteAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false);
        }
    }
}
