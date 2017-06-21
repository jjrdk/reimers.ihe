namespace Reimers.Ihe.Communication
{
    using System.Net;
    using System.Text;

    /// <summary>
    /// Class ClientConnectionDetails. Defines a DTO to keep the connection details for a client.
    /// </summary> 
    public class ClientConnectionDetails
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClientConnectionDetails"/> class.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="port">The port.</param>
        /// <param name="encoding">The encoding. default is ASCII.</param>
        /// <param name="reconnectionDetails">Reconnection behavior related properties.</param>
        /// <param name="clientSecurityDetails">Specifies optional, additional security details (server auth and mutual auth).</param>
        public ClientConnectionDetails(string address, int port, Encoding encoding = null, ReconnectionDetails reconnectionDetails = null, ClientSecurityDetails clientSecurityDetails = null)
        {
            this.Address = address;
            this.Port = port;
            this.Encoding = encoding ?? Encoding.ASCII;
            this.ReconnectionDetails = reconnectionDetails ?? new ReconnectionDetails();
            this.SecurityDetails = clientSecurityDetails;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientConnectionDetails"/> class.
        /// </summary>
        /// <param name="ipEndPoint">The IP Endpoint.</param>
        /// <param name="encoding">The encoding. default is ASCII.</param>
        /// <param name="reconnectionDetails">Reconnection behavior related properties.</param>
        /// <param name="clientSecurityDetails">Specifies optional, additional security details (server auth and mutual auth).</param>
        public ClientConnectionDetails(IPEndPoint ipEndPoint, Encoding encoding = null, ReconnectionDetails reconnectionDetails = null, ClientSecurityDetails clientSecurityDetails = null)
            : this(ipEndPoint.Address.ToString(), ipEndPoint.Port, encoding, reconnectionDetails, clientSecurityDetails)
        {
        }

        /// <summary>
        /// Gets the address.
        /// </summary>
        /// <value>The address.</value>
        public string Address { get; }

        /// <summary>
        /// Gets the port.
        /// </summary>
        /// <value>The port.</value>
        public int Port { get; }

        /// <summary>
        /// Gets the encoding.
        /// </summary>
        /// <value>The encoding.</value>
        public Encoding Encoding { get; }

        /// <summary>
        /// Gets the reconnection details.
        /// </summary>
        /// <value>The reconnection details.</value>
        public ReconnectionDetails ReconnectionDetails { get; }

        /// <summary>
        /// Gets the security details to be used by the client.
        /// </summary>
        /// <value>The security details.</value>
        public ClientSecurityDetails SecurityDetails { get; }
    }
}
