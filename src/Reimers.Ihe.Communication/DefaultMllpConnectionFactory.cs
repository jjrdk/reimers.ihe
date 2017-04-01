namespace Reimers.Ihe.Communication
{
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines the default MLLP connection factory.
    /// </summary>
    public class DefaultMllpConnectionFactory
    {
        private readonly string _address;
        private readonly int _port;
        private readonly Encoding _encoding;
        private readonly X509CertificateCollection _clientCertificateCollection;

        /// <summary>
        /// Iniitializes a new instance of the <see cref="DefaultMllpConnectionFactory"/> class.
        /// </summary>
        /// <param name="address">The address of the remote server.</param>
        /// <param name="port">The port of the remote server.</param>
        /// <param name="encoding">The <see cref="Encoding"/> to use for data transfer. If no value is specified, then <see cref="Encoding.ASCII"/> is used.</param>
        /// <param name="clientCertificateCollection">The client certificates to use for connection security.</param>
        public DefaultMllpConnectionFactory(string address, int port, Encoding encoding = null,
            X509CertificateCollection clientCertificateCollection = null)
        {
            _address = address;
            _port = port;
            _encoding = encoding;
            _clientCertificateCollection = clientCertificateCollection;
        }

        /// <summary>
        /// Gets an instance of an MLLP connection.
        /// </summary>
        /// <returns>An MLLP connection as an async operation.</returns>
        public Task<IHostConnection> Get()
        {
            return MllpClient.Create(_address, _port, _encoding, _clientCertificateCollection);
        }
    }
}