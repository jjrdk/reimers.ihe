namespace Reimers.Ihe
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines the interface for a connection to an IHE host.
    /// </summary>
    public interface IHostConnection : IDisposable
    {
        /// <summary>
        /// Sends the passed message to the server and awaits the response.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the operation.</param>
        /// <returns>An <see cref="Hl7Message"/> containing the response and source address.</returns>
        Task<Hl7Message> Send(
            string message,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}