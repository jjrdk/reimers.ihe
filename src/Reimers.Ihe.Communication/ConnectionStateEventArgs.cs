namespace Reimers.Ihe.Communication
{
    using System;

    /// <summary>
    /// Class ConnectionStateEventArgs. Defines the event args for an event that will be raised when the connection state is changed.
    /// </summary>
    /// <seealso cref="System.EventArgs" />
    public class ConnectionStateEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the current connection status.
        /// </summary>
        public ConnectionStatus ConnectionStatus { get; set; }
    }
}
