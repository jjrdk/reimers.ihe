namespace Reimers.Ihe.Communication
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Security.Authentication;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines an IHE server using MLLP connections.
    /// </summary>
    public class MllpServer : IDisposable
    {
        private readonly IHl7MessageMiddleware _middleware;
        private readonly TcpListener _listener;
        private readonly List<MllpHost> _connections = new List<MllpHost>();
        private readonly Timer _timer;
        private readonly CancellationToken _token;
        private readonly ServerConnectionDetails _connectionDetails;


        public MllpServer(ServerConnectionDetails connectionDetails, IHl7MessageMiddleware middleware, CancellationToken cancellationToken = default(CancellationToken))
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(connectionDetails.Address), connectionDetails.Port);

            _token = cancellationToken;
            _connectionDetails = connectionDetails;
            _middleware = middleware;
            _listener = new TcpListener(endPoint);
            _timer = new Timer(CleanConnections, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
        }

        public event EventHandler<ClientConnectionEventArgs> OnClientConnected;

        public event EventHandler<ClientConnectionEventArgs> OnClientAuthenticationFailed;

        public IPEndPoint EndPoint => (IPEndPoint)_listener.LocalEndpoint;

        /// <summary>
        /// Starts the server
        /// </summary>
        public void Start()
        {
            _listener.Start();
            Read();
        }

        /// <summary>
        /// Stops the server
        /// </summary>
        public void StopListening()
        {
            _listener.Stop();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _listener.Stop();
            _timer.Dispose();
            lock (_connections)
            {
                foreach (var connection in _connections)
                {
                    connection.Dispose();
                }
                _connections.Clear();
            }
        }

        private void Read()
        {
            Task.Run(async () =>
                {
                    while (!_token.IsCancellationRequested)
                    {
                        TcpClient client = await _listener.AcceptTcpClientAsync();
                        await this.HandleClientRequestAsync(client);
                    }
                });
        }

        private void CleanConnections(object o)
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
            foreach (var host in temp)
            {
                host.Dispose();
            }
        }

        private async Task HandleClientRequestAsync(TcpClient client)
        {
            var clientEndpoint = (IPEndPoint)client.Client.RemoteEndPoint;

            MllpHost connection;
            try
            {
                connection = await MllpHost.Create(client, _middleware, _connectionDetails.Encoding, _connectionDetails.SecurityDetails, _token);
            }
            catch (AuthenticationException)
            {
                // don't bring down server just because a client wasn't table to authenticate.
                this.TriggerClientAuthenticationFailedEvent(clientEndpoint);
                return;
            }

            lock (_connections)
            {
                _connections.Add(connection);
            }

            this.TriggerClientConnectedEvent(clientEndpoint);
        }

        /// <summary>
        /// Triggers the client connected event.
        /// </summary>
        /// <param name="clientEndPoint">The end-point of the client that connected.</param>
        private void TriggerClientConnectedEvent(IPEndPoint clientEndPoint)
        {
            this.OnClientConnected?.Invoke(
                this,
                new ClientConnectionEventArgs
                {
                    EndPoint = clientEndPoint
                });
        }

        /// <summary>
        /// Triggers the client connection state changed.
        /// </summary>
        /// <param name="clientEndPoint">The end-point of the client that connected.</param>
        private void TriggerClientAuthenticationFailedEvent(IPEndPoint clientEndPoint)
        {
            this.OnClientAuthenticationFailed?.Invoke(
                this,
                new ClientConnectionEventArgs
                {
                    EndPoint = clientEndPoint
                });
        }
    }
}
