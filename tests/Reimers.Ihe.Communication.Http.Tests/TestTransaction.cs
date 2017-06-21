namespace Reimers.Ihe.Communication.Http.Tests
{
    using System.Threading.Tasks;
    using NHapi.Base.Parser;
    using NHapi.Model.V251.Message;

    public class TestTransaction : LocallyInitiatedIheTransaction<QBP_Q11, ACK>
    {
        public TestTransaction(Task<IHostConnection> connectionFactory, PipeParser parser, ClientConnectionDetails clientConnectionDetails)
            : base(() => connectionFactory, parser, clientConnectionDetails)
        {
        }
    }
}