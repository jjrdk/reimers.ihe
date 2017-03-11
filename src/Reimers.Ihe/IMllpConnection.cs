namespace Reimers.Ihe
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IMllpConnection : IDisposable
    {
        Task<Hl7Message> Send(
            string message,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}