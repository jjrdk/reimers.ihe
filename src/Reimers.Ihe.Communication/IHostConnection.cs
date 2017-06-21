namespace Reimers.Ihe.Communication
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
        /// Defines an event that will be raised every time the connection state is changed.
        /// </summary>
        event EventHandler<ConnectionStateEventArgs> OnConnectionStateChanged;

        /// <summary> 
        /// Sends the passed message to the server and awaits the response. 
        /// </summary> 
        /// <param name="message">The message to send.</param> 
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the operation.</param> 
        /// <returns>A string containing the response and source address.</returns> 
        Task<Hl7Message> Send(string message, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Determines whether the connection is established or not.
        /// </summary>
        /// <returns>True if the connection is established.</returns>
        bool IsConnectionEstablished();
    }
}