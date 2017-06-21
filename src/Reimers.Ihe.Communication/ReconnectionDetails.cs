namespace Reimers.Ihe.Communication
{
    /// <summary>
    /// Class ReconnectionDetails. Defines a DTO to keep the client-side reconnection behavior.
    /// </summary>
    public class ReconnectionDetails
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReconnectionDetails"/> class.
        /// </summary>
        /// <param name="automaticReConnection">Specifies whether the connection needs to be reestablished automatically when lost.</param>
        /// <param name="maxReconnectionAttempts">Specifies how many times connections will be tried to be reestablished when lost. Use -1 for continuous attempts.</param>
        /// <param name="reconnectionTimeoutInMilliseconds">Specifies the amount of time (in milliseconds) to wait until it tries to reconnect.</param>
        public ReconnectionDetails(bool automaticReConnection = false, int maxReconnectionAttempts = 0, int reconnectionTimeoutInMilliseconds = 0)
        {
            this.AutomaticReconnection = automaticReConnection;
            this.MaxReconnectionAttempts = maxReconnectionAttempts;
            this.ReconnectionTimeoutInMilliseconds = reconnectionTimeoutInMilliseconds;
        }

        /// <summary>
        /// Gets a value indicating whether the connection needs to be automatically reestablished when broken.
        /// </summary>
        public bool AutomaticReconnection { get; }

        /// <summary>
        /// Gets the maximum reconnection attempts.
        /// </summary>
        /// <value>The maximum reconnection attempts.</value>
        public int MaxReconnectionAttempts { get; }

        /// <summary>
        /// Gets the reconnection timeout in milliseconds.
        /// </summary>
        /// <value>The reconnection timeout in milliseconds.</value>
        public int ReconnectionTimeoutInMilliseconds { get; }
    }
}
