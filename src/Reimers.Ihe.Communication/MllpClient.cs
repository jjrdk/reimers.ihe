namespace Reimers.Ihe.Communication
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Security;
    using System.Net.Sockets;
    using System.Security;
    using System.Security.Authentication;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading;
    using System.Threading.Tasks;

    public class MllpClient : IHostConnection
    {
        /// <summary>
        /// The connection details.
        /// </summary>
        private readonly ClientConnectionDetails connectionDetails;

        /// <summary>
        /// The completion source.
        /// </summary>
        private readonly TaskCompletionSource<Hl7Message> completionSource = new TaskCompletionSource<Hl7Message>();

        /// <summary>
        /// The token source.
        /// </summary>
        private readonly CancellationTokenSource tokenSource = new CancellationTokenSource();

        /// <summary>
        /// The stream.
        /// </summary>
        private Stream stream;

        /// <summary>
        /// The tcp client.
        /// </summary>
        private TcpClient tcpClient;

        /// <summary>
        /// The sending.
        /// </summary>
        private bool sending;

        /// <summary>
        /// The reading stream.
        /// </summary>
        private bool readingStream;

        /// <summary>
        /// Initializes a new instance of the <see cref="MllpClient"/> class.
        /// </summary>
        /// <param name="connectionDetails">Provides all the connection details.</param>
        public MllpClient(ClientConnectionDetails connectionDetails)
        {
            this.connectionDetails = connectionDetails;
        }

        /// <inheritdoc />
        public event EventHandler<ConnectionStateEventArgs> OnConnectionStateChanged;

        /// <summary>
        /// The reconnection details.
        /// </summary>
        private ReconnectionDetails ReconnectionDetails => this.connectionDetails.ReconnectionDetails;

        /// <summary>
        /// The security details.
        /// </summary>
        private ClientSecurityDetails SecurityDetails => this.connectionDetails.SecurityDetails;

        /// <summary>
        /// Creates a connection to the specified address.
        /// </summary>
        /// <param name="connectionDetails">Provides all the connection details.</param>
        /// <returns>A Task&lt;IHostConnection&gt;.</returns>
        public static async Task<IHostConnection> Create(ClientConnectionDetails connectionDetails)
        {
            var instance = new MllpClient(connectionDetails);
            await instance.Setup();
            return instance;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.tokenSource.Cancel();
            this.stream?.Dispose();
            this.tcpClient.Dispose();
        }

        /// <summary>
        /// Sends the passed message to the server and awaits the response.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken" /> for the operation.</param>
        /// <returns>A string containing the response and source address.</returns>
        /// <exception cref="System.InvalidOperationException">Transaction ongoing</exception>
        public async Task<Hl7Message> Send(string message, CancellationToken cancellationToken = default(CancellationToken))
        {
            lock (this.stream)
            {
                if (this.sending)
                {
                    throw new InvalidOperationException("Transaction ongoing");
                }
                this.sending = true;
            }
            var bytes = this.connectionDetails.Encoding.GetBytes(message);
            var buffer = new byte[bytes.Length + Constants.StartBlock.Length + Constants.EndBlock.Length];
            Array.Copy(Constants.StartBlock, 0, buffer, 0, Constants.StartBlock.Length);
            Array.Copy(bytes, 0, buffer, Constants.StartBlock.Length, bytes.Length);
            Array.Copy(Constants.EndBlock, 0, buffer, Constants.StartBlock.Length + bytes.Length, Constants.EndBlock.Length);

            await this.stream.WriteAsync(buffer, 0, buffer.Length, cancellationToken);
            this.ReadStream(this.tokenSource.Token);
            return await this.completionSource.Task;
        }

        /// <summary>
        /// Determines whether the connection is established or not.
        /// </summary>
        /// <returns>True if the connection is established.</returns>
        public bool IsConnectionEstablished()
        {
            return this.tcpClient != null && (!this.tcpClient.Client.Poll(1000, SelectMode.SelectRead) || this.tcpClient.Client.Available != 0);
        }

        /// <summary>
        /// Setups this instance.
        /// </summary>
        /// <returns>A Task.</returns>
        private async Task Setup()
        {
            this.tcpClient = new TcpClient();
            await this.tcpClient.ConnectAsync(IPAddress.Parse(this.connectionDetails.Address), this.connectionDetails.Port);

            if (this.connectionDetails.SecurityDetails != null)
            {
                this.stream = await this.CreateSslStreamAsync(this.tcpClient.GetStream());
            }
            else
            {
                this.stream = this.tcpClient.GetStream();
            }

            this.TriggerConnectionStateChanged(ConnectionStatus.Connected);

            this.StartConnectionMonitoring();
        }

        /// <summary>
        /// Creates an SSL stream to run encrypted communication with the server.
        /// Authenticates the server, and provides client-certificates if asked for by the server. 
        /// </summary>
        /// <param name="innerStream">
        /// The inner Stream.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        private async Task<SslStream> CreateSslStreamAsync(Stream innerStream)
        {
            var sslStream = new SslStream(innerStream, false, this.SecurityDetails.ServerCertificateValidationCallback, this.SecurityDetails.ClientCertificateSelectionCallback, EncryptionPolicy.RequireEncryption);

            try
            {
                X509CertificateCollection clientCetrificates = this.SecurityDetails.ClientCertificates ?? new X509CertificateCollection();
                await sslStream.AuthenticateAsClientAsync(this.connectionDetails.Address, clientCetrificates, this.SecurityDetails.SupportedSslProtocols, false);

                // make sure we are operating in the expected setup:
                if (!sslStream.IsEncrypted)
                {
                    throw new SecurityException("the ssl stream is not encrypted.");
                }

                if (!sslStream.IsAuthenticated)
                {
                    throw new AuthenticationException("server authentication failed.");
                }

                if (clientCetrificates.Count > 0 && !sslStream.IsMutuallyAuthenticated)
                {
                    throw new AuthenticationException("mutual authentication failed.");
                }

                return sslStream;
            }
            catch (Exception)
            {
                sslStream.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Starts checking whether the connection state has change or not.
        /// </summary>
        private void StartConnectionMonitoring()
        {
            Task.Run(async () =>
                {
                    while (this.IsConnectionEstablished())
                    {
                        Thread.Sleep(500);
                    }

                    this.TriggerConnectionStateChanged(ConnectionStatus.Disconnected);

                    if (this.ReconnectionDetails.AutomaticReconnection)
                    {
                        await this.TryReconnection();
                    }
                });
        }

        /// <summary>
        /// Tries to reestablish the connection.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        private async Task TryReconnection()
        {
            int numberOfAttempts = 0;
            int maxNumberOfAttempts = this.ReconnectionDetails.MaxReconnectionAttempts > 0 ? this.ReconnectionDetails.MaxReconnectionAttempts : int.MaxValue;
            int elapseTime = this.ReconnectionDetails.ReconnectionTimeoutInMilliseconds > 0 ? this.ReconnectionDetails.ReconnectionTimeoutInMilliseconds : 1000;

            do
            {
                try
                {
                    await this.Setup();

                    if (this.IsConnectionEstablished())
                    {
                        if (this.readingStream)
                        {
                            this.ReadStream(this.tokenSource.Token);
                        }
                        break;
                    }
                }
                catch
                {
                    this.tcpClient = null;
                }

                numberOfAttempts++;
                Thread.Sleep(elapseTime);
            }
            while (numberOfAttempts < maxNumberOfAttempts);
        }

        /// <summary>
        /// Reads the stream and returns the received message.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <exception cref="Exception">When none expected characters are received.</exception>
        private void ReadStream(CancellationToken cancellationToken)
        {
            try
            {
                byte previous = 0;
                this.readingStream = true;
                var messageBuilder = new List<byte>();
                while (this.IsConnectionEstablished())
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var current = (byte)this.stream.ReadByte();
                    if (Constants.EndBlock.SequenceEqual(new[] { previous, current }))
                    {
                        messageBuilder.RemoveAt(messageBuilder.Count - 1);
                        var responseAsString = this.connectionDetails.Encoding.GetString(messageBuilder.ToArray());
                        var message = new Hl7Message(responseAsString, this.tcpClient.Client.RemoteEndPoint.ToString());
                        this.completionSource.SetResult(message);
                        this.readingStream = false;
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
                this.completionSource.SetException(io);
                this.readingStream = false;
            }
        }

        /// <summary>
        /// Triggers the connection state changed.
        /// </summary>
        /// <param name="connectionStatus">The connection status.</param>
        private void TriggerConnectionStateChanged(ConnectionStatus connectionStatus)
        {
            this.OnConnectionStateChanged?.Invoke(
                this,
                new ConnectionStateEventArgs
                    {
                        ConnectionStatus = connectionStatus
                    });
        }
    }
}