using System.Threading.Tasks;
using NHapi.Model.V251.Message;
using Reimers.Ihe.Abstractions;

namespace Reimers.Ihe.Communication.Tests;

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