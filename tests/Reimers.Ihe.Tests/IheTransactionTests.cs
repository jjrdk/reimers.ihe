namespace Reimers.Ihe.Tests
{
    using System;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;
    using NHapi.Base.Parser;
    using NHapi.Model.V251.Message;
    using Xunit;

    public class IheTransactionTests : IDisposable
    {
        private readonly MllpServer _server;
        private int _port = 2575;

        public IheTransactionTests()
        {
            _server = new MllpServer(new IPEndPoint(IPAddress.Loopback, _port), new TestMiddleware());
            _server.Start();
        }

        [Fact]
        public async Task WhenSendingMessageThenGetsAck()
        {
            var connectionFactory = new DefaultMllpConnectionFactory(IPAddress.Loopback.ToString(), _port);
            var client = new TestTransaction(connectionFactory.Get, new PipeParser());
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
