namespace Reimers.Ihe.Communication.Tests
{
    using System;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;
    using NHapi.Base.Parser;
    using NHapi.Model.V251.Message;
    using Xunit;

    public class MllpServerTests : IDisposable
    {
        private MllpServer _server;
        private PipeParser _parser;

        public MllpServerTests()
        {
            _parser = new PipeParser();
            _server = new MllpServer(
                new IPEndPoint(IPAddress.IPv6Loopback, 2575),
                NullLog.Get(),
                new DefaultHl7MessageMiddleware(
                    handlers: new TestTransactionHandler()),
                Encoding.ASCII);
            _server.Start();
        }

        [Fact]
        public async Task WhenClientSendsMessageToServerThenReceivesResponse()
        {
            var address = IPAddress.IPv6Loopback.ToString();
            var client = await MllpClient.Create(
                    address,
                    2575,
                    NullLog.Get(),
                    Encoding.ASCII)
                .ConfigureAwait(false);
            var adt = new ADT_A01();
            adt.MSH.MessageControlID.Value =
                DefaultMessageControlIdGenerator.Instance.NextId();

            var msg = _parser.Encode(adt);

            var response = await client.Send(msg).ConfigureAwait(false);

            var ack = _parser.Parse(response.Message);

            Assert.NotNull(ack);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _server?.Dispose();
        }
    }
}
