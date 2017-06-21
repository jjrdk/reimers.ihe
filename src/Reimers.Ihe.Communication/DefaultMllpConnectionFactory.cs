namespace Reimers.Ihe.Communication
{
    using System.Threading.Tasks;

    /// <summary>
    /// Defines the default MLLP connection factory.
    /// </summary>
    public class DefaultMllpConnectionFactory
    {
        /// <summary>
        /// Gets an instance of an MLLP connection.
        /// </summary>
        /// <param name="connectionDetails">
        /// The connection Details.
        /// </param>
        /// <returns>
        /// An MLLP connection as an async operation.
        /// </returns>
        public Task<IHostConnection> GetClientConnection(ClientConnectionDetails connectionDetails)
        {
            return MllpClient.Create(connectionDetails);
        }

        public MllpServer CreateServerConnection(ServerConnectionDetails connectionDetails, IHl7MessageMiddleware middleware)
        {
            return new MllpServer(connectionDetails, middleware);
        }
    }
}