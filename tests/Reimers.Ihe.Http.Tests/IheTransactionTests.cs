namespace Reimers.Ihe.Http.Tests
{
    using System;
    using System.Threading.Tasks;
    using NHapi.Base.Parser;
    using NHapi.Model.V251.Message;
    using Xunit;

    public class IheTransactionTests : IDisposable
    {
        private readonly IheHttpServer _server;
        private int _port = 8080;

        public IheTransactionTests()
        {
            _server = new IheHttpServer(new[] { "http://localhost:8080" }, new TestMiddleware());
        }

        [Fact]
        public async Task WhenSendingMessageThenGetsAck()
        {
            var connectionFactory = new DefaultHttpConnectionFactory(new Uri("http://localhost:8080"));
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
