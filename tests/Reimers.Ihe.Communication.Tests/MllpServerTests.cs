namespace Reimers.Ihe.Communication.Tests
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using Abstractions;
    using NHapi.Model.V251.Message;
    using Xunit;

    public class MllpServerTests : IDisposable
    {
        private readonly MllpServer _server;

        public MllpServerTests()
        {
            _server = new MllpServer(
                new IPEndPoint(IPAddress.IPv6Loopback, 2575),
                NullLog.Get(),
                new DefaultHl7MessageMiddleware(
                    handlers: new TestTransactionHandler()),
                TimeSpan.FromMilliseconds(100));
            _server.Start();
        }

        [Fact]
        public async Task WhenClientSendsMessageToServerThenReceivesResponse()
        {
            var address = IPAddress.IPv6Loopback.ToString();
            await using var client = await MllpClient.Create(
                     address,
                     2575)
                 .ConfigureAwait(false);
            var adt = new ADT_A01();
            adt.MSH.MessageControlID.Value =
                DefaultMessageControlIdGenerator.Instance.NextId();

            var response = await client.Send(adt).ConfigureAwait(false);

            Assert.NotNull(response.Message);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            _server?.DisposeAsync().AsTask().Wait();
        }
    }
}
