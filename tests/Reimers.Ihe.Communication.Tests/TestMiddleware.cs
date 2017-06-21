namespace Reimers.Ihe.Communication.Tests
{
    using System.Threading.Tasks;
    using NHapi.Base.Parser;
    using NHapi.Model.V251.Message;

    public class TestMiddleware : IHl7MessageMiddleware
    {
        private readonly PipeParser _parser = new PipeParser();

        public Task<string> Handle(Hl7Message message)
        {
            var ack = new ACK();
            var hl7 = _parser.Encode(ack);

            return Task.FromResult(hl7);
        }
    }
}