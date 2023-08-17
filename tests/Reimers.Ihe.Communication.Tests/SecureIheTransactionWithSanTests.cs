namespace Reimers.Ihe.Communication.Tests
{
    using System;
    using System.Net;
    using System.Net.Security;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading.Tasks;
    using NHapi.Model.V251.Message;
    using Xunit;

    public class SecureIheTransactionWithSanTests : IAsyncDisposable
    {
        private readonly MllpServer _server;
        private readonly int _port = 2576;
        private readonly X509Certificate2Collection _cert;

        public SecureIheTransactionWithSanTests()
        {
            var certificate =
                X509Certificate2.CreateFromPemFile("cert.pem", "san.pem");
            var buffer = certificate.Export(X509ContentType.Pfx, (string)null);
            certificate = new X509Certificate2(buffer, (string)null);
            _cert = new X509Certificate2Collection(
                certificate);
            _server = new MllpServer(
                new IPEndPoint(IPAddress.Loopback, _port),
                NullLog.Get(),
                new TestMiddleware(),
                serverCertificate: _cert[0],
                userCertificateValidationCallback:
                UserCertificateValidationCallback);
            _server.Start();
        }

        private static bool UserCertificateValidationCallback(
            object sender,
            X509Certificate certificate,
            X509Chain chain,
            SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        [Fact]
        public async Task WhenSendingMessageThenGetsAck()
        {
            var client = await MllpClient.Create(
                    IPAddress.Loopback.ToString(),
                    _port,
                    clientCertificates: _cert,
                    userCertificateValidationCallback:
                    UserCertificateValidationCallback)
                .ConfigureAwait(false);
            var request = new QBP_Q11
            {
                MSH = { MessageControlID = { Value = "test" }}
            };

            var response = await client.Send(request).ConfigureAwait(false);
            Assert.NotNull(response);
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            GC.SuppressFinalize(this);
            await _server.DisposeAsync();
        }
    }
}
