namespace Reimers.Ihe.Communication.Tests
{
    using System;
    using System.Threading.Tasks;
    using NHapi.Base.Parser;
    using NHapi.Model.V251.Message;    public class TestTransaction : IheTransaction<QBP_Q11, ACK>
    {
        public TestTransaction(Func<Task<IHostConnection>> connectionFactory, PipeParser parser)
            : base(connectionFactory, parser)
        {
        }
    }
}