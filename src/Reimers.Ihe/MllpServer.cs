namespace Reimers.Ihe
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    public class MllpServer : IDisposable
    {
        private readonly IHl7MessageMiddleware _middleware;
        private readonly Encoding _encoding;
        private readonly X509Certificate _serverCertificate;
        private readonly TcpListener _listener;
        private readonly List<MllpHost> _connections = new List<MllpHost>();
        private readonly Timer _timer;
        private readonly CancellationTokenSource _tokenSource = new CancellationTokenSource();
        private Task _readTask;

        public MllpServer(IPEndPoint endPoint, IHl7MessageMiddleware middleware, Encoding encoding = null, X509Certificate serverCertificate = null)
        {
            _middleware = middleware;
            _encoding = encoding;
            _serverCertificate = serverCertificate;
            _listener = new TcpListener(endPoint);
            _timer = new Timer(CleanConnections, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
        }

        public void Start()
        {
            _listener.Start();
            _readTask = Read();
        }

        public void Dispose()
        {
            _tokenSource.Cancel();
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

        private async Task Read()
        {
            while (!_tokenSource.Token.IsCancellationRequested)
            {
                var client = await _listener.AcceptTcpClientAsync().ConfigureAwait(false);

                var connection = await MllpHost.Create(client, _middleware, _encoding, _serverCertificate);
                lock (_connections)
                {
                    _connections.Add(connection);
                }
            }
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
    }
}