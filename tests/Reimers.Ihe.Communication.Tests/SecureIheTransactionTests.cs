namespace Reimers.Ihe.Communication.Tests
{
    using System;
    using System.Net;
    using System.Net.Security;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading.Tasks;
    using NHapi.Base.Parser;
    using NHapi.Model.V251.Message;
    using Xunit;

    public class SecureIheTransactionTests : IDisposable
    {
        private readonly MllpServer _server;
        private int _port = 2575;
        private readonly X509Certificate2Collection _cert;

        public SecureIheTransactionTests()
        {
            _cert = new X509Certificate2Collection(new X509Certificate2("cert.pfx", "password"));
            _server = new MllpServer(new IPEndPoint(IPAddress.Loopback, _port), new TestMiddleware(), serverCertificate: _cert[0], userCertificateValidationCallback: UserCertificateValidationCallback);
            _server.Start();
        }

        private bool UserCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        [Fact]
        public async Task WhenSendingMessageThenGetsAck()
        {
            var connectionFactory = new DefaultMllpConnectionFactory(IPAddress.Loopback.ToString(), _port, clientCertificateCollection: _cert, userCertificateValidationCallback: UserCertificateValidationCallback);
            var client = new TestTransaction(connectionFactory.Get, new PipeParser());
            var request = new QBP_Q11();
            var response = await client.Send(request); Assert.NotNull(response);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _server?.Dispose();
        }
    }
}