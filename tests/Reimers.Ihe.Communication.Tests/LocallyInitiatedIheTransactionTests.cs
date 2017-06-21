namespace Reimers.Ihe.Communication.Tests
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using NHapi.Base.Parser;
    using NHapi.Model.V251.Message;
    using Xunit;

    public class LocallyInitiatedIheTransactionTests : IDisposable
    {
        private readonly MllpServer _server;
        private int _port = 2575;
        
        public LocallyInitiatedIheTransactionTests()
        {
            ServerConnectionDetails serverConnection = new ServerConnectionDetails(new IPEndPoint(IPAddress.Loopback, _port));

            _server = new MllpServer(serverConnection, new TestMiddleware());
            _server.Start();
        }

        [Fact]
        public async Task WhenSendingMessageThenGetsAck()
        {
            ClientConnectionDetails clientConnectionDetails = new ClientConnectionDetails(new IPEndPoint(IPAddress.Loopback, _port));

            var connectionFactory = new DefaultMllpConnectionFactory();
            var client = new TestTransaction(connectionFactory.GetClientConnection(clientConnectionDetails), new PipeParser(), clientConnectionDetails);
            var request = new QBP_Q11();
            var response = await client.Send(request);

            Assert.NotNull(response);
        }        
        
        /// <inheritdoc />
        public void Dispose()
        {
            _server?.Dispose();
        }
    }
}
