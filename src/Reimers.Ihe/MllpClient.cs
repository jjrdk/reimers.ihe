namespace Reimers.Ihe
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

    internal class MllpClient : IMllpConnection
    {
        private readonly string _address;
        private readonly int _port;
        private readonly Encoding _encoding;
        private readonly X509CertificateCollection _clientCertificates;
        private string _remoteAddress;
        private readonly TaskCompletionSource<Hl7Message> _completionSource = new TaskCompletionSource<Hl7Message>();
        private Stream _stream;
        private readonly CancellationTokenSource _tokenSource = new CancellationTokenSource();
        private TcpClient _tcpClient;
        private Task _readThread;

        private MllpClient(string address, int port, Encoding encoding, X509CertificateCollection clientCertificates)
        {
            _address = address;
            _port = port;
            _encoding = encoding;
            _clientCertificates = clientCertificates;
        }

        public static async Task<IMllpConnection> Create(
            string address,
            int port,
            Encoding encoding = null,
            X509CertificateCollection clientCertificates = null)
        {
            var instance = new MllpClient(address, port, encoding ?? Encoding.ASCII, clientCertificates);
            await instance.Setup().ConfigureAwait(false);

            return instance;
        }

        public async Task<Hl7Message> Send(string message, CancellationToken cancellationToken = default(CancellationToken))
        {
            var buffer =
                Constants.StartBlock.Concat(_encoding.GetBytes(message)).Concat(Constants.EndBlock).ToArray();
            await _stream.WriteAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false);
            return await _completionSource.Task;
        }

        private async Task Setup()
        {
            _tcpClient = new TcpClient(_address, _port);
            _remoteAddress = _tcpClient.Client.RemoteEndPoint.ToString();
            _stream = _tcpClient.GetStream();
            if (_clientCertificates != null)
            {
                var ssl = new SslStream(_stream, false);

                await ssl.AuthenticateAsClientAsync(_address, _clientCertificates, SslProtocols.Default | SslProtocols.Tls12, false).ConfigureAwait(false);
                _stream = ssl;
            }

            _readThread = ReadStream(_tokenSource.Token);
        }

        public void Dispose()
        {
            _tokenSource.Cancel();
            _stream.Dispose();
            _tcpClient.Dispose();
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
                        var s = _encoding.GetString(messageBuilder.ToArray());

                        var message = new Hl7Message(s, _remoteAddress);
                        _completionSource.SetResult(message);
                        break;
                    }

                    if (previous == 0 && current == 11)
                    {
                        if (messageBuilder.Count > 0)
                        {
                            throw new Exception($"Unexpected character: {current:x2}");
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
                _completionSource.SetException(io);
            }
        }
    }
}