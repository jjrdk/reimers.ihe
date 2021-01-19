namespace Reimers.Ihe.Communication.Tests
{
    using System.Threading.Tasks;
    using NHapi.Model.V251.Message;

    public class TestTransactionHandler : IheTransactionHandler<ADT_A01, ACK>
    {
        public override string Handles => nameof(ADT_A01);

        public override string Version => "2.5.1";

        protected override Task<ACK> HandleInternal(ADT_A01 message)
        {
            var result = new ACK();
            result.MSH.MessageControlID.Value = message.MSH.MessageControlID.Value;
            result.MSA.MessageControlID.Value = message.MSH.MessageControlID.Value;

            return Task.FromResult(result);
        }
    }

    public class TestDischargeTransactionHandler : IheTransactionHandler<ADT_A03, ACK>
    {
        public override string Handles => nameof(ADT_A03);

        public override string Version => "2.5.1";

        protected override Task<ACK> HandleInternal(ADT_A03 message)
        {
            var result = new ACK();
            result.MSH.MessageControlID.Value = message.MSH.MessageControlID.Value;
            result.MSA.MessageControlID.Value = message.MSH.MessageControlID.Value;

            return Task.FromResult(result);
        }
    }

    public class TestOruTransactionHandler : IheTransactionHandler<ORU_R01, ACK>
    {
        public override string Handles => nameof(ORU_R01);

        public override string Version => "2.5.1";

        protected override Task<ACK> HandleInternal(ORU_R01 message)
        {
            var result = new ACK();
            result.MSH.MessageControlID.Value = message.MSH.MessageControlID.Value;
            result.MSA.MessageControlID.Value = message.MSH.MessageControlID.Value;

            return Task.FromResult(result);
        }
    }
}