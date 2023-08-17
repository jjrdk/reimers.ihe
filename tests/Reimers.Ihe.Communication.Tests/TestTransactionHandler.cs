namespace Reimers.Ihe.Communication.Tests
{
    using System.Threading.Tasks;
    using Abstractions;
    using NHapi.Model.V251.Message;

    public class TestTransactionHandler : IheTransactionHandler<ADT_A01, ACK>
    {
        public override string Handles => nameof(ADT_A01);

        public override string Version => "2.5.1";

        protected override Task<ACK> HandleInternal(ADT_A01 message)
        {
            var result = new ACK
            {
                MSH = { MessageControlID = { Value =message.MSH.MessageControlID.Value }},
                MSA = { MessageControlID = { Value =message.MSH.MessageControlID.Value }}
            };

            return Task.FromResult(result);
        }
    }
}
