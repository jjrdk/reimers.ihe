namespace Reimers.Ihe.Http
{
    using System;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines the default MLLP connection factory.
    /// </summary>
    public class DefaultHttpConnectionFactory
    {
        private readonly Uri _address;
        private readonly Encoding _encoding;
        private readonly X509CertificateCollection _clientCertificateCollection;

        /// <summary>
        /// Iniitializes a new instance of the <see cref="DefaultHttpConnectionFactory"/> class.
        /// </summary>
        /// <param name="address">The address of the remote server.</param>
        /// <param name="encoding">The <see cref="Encoding"/> to use for data transfer. If no value is specified, then <see cref="Encoding.ASCII"/> is used.</param>
        /// <param name="clientCertificateCollection">The client certificates to use for connection security.</param>
        public DefaultHttpConnectionFactory(Uri address, Encoding encoding = null, X509CertificateCollection clientCertificateCollection = null)
        {
            _address = address;
            _encoding = encoding;
            _clientCertificateCollection = clientCertificateCollection;
        }

        /// <summary>
        /// Gets an instance of an HTTP connection.
        /// </summary>
        /// <returns>An MLLP connection as an async operation.</returns>
        public Task<IHostConnection> Get()
        {
            return Task.FromResult<IHostConnection>(new IheHttpClient(_address, _encoding, _clientCertificateCollection));
        }
    }
}