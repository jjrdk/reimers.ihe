using System.Threading;
using System.Threading.Tasks;
using NHapi.Model.V251.Message;
using Reimers.Ihe.Abstractions;

namespace Reimers.Ihe.Communication.Tests;

public class TestDischargeTransactionHandler : IheTransactionHandler<ADT_A03, ACK>
{
    public override string Handles => nameof(ADT_A03);

    public override string Version => "2.5.1";

    protected override Task<ACK> HandleInternal(ADT_A03 message, CancellationToken cancellationToken = default)
    {
        var result = new ACK();
        result.MSH.MessageControlID.Value = message.MSH.MessageControlID.Value;
        result.MSA.MessageControlID.Value = message.MSH.MessageControlID.Value;

        return Task.FromResult(result);
    }
}
