namespace Reimers.Ihe.Communication
{
    using System.Net;
    using System.Text;

    /// <summary>
    /// Class ServerConnectionDetails. Defines a DTO to keep the details of a connection.
    /// </summary>
    public class ServerConnectionDetails
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServerConnectionDetails"/> class.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="port">The port.</param>
        /// <param name="encoding">The encoding. default is ASCII.</param>
        /// <param name="securityDetails">The security details to be used by the server (if any).</param>
        public ServerConnectionDetails(string address, int port, Encoding encoding = null, ServerSecurityDetails securityDetails = null)
        {
            this.Address = address;
            this.Port = port;
            this.Encoding = encoding ?? Encoding.ASCII;
            this.SecurityDetails = securityDetails;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerConnectionDetails"/> class.
        /// </summary>
        /// <param name="ipEndPoint">The IP Endpoint.</param>
        /// <param name="encoding">The encoding. default is ASCII.</param>
        /// <param name="securityDetails">The security details to be used by the server (if any).</param>
        public ServerConnectionDetails(IPEndPoint ipEndPoint, Encoding encoding = null, ServerSecurityDetails securityDetails = null)
            : this(ipEndPoint.Address.ToString(), ipEndPoint.Port, encoding, securityDetails)
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
        /// Gets the security details to be used by the server.
        /// </summary>
        /// <value>The security details.</value>
        public ServerSecurityDetails SecurityDetails { get; }
    }
}
