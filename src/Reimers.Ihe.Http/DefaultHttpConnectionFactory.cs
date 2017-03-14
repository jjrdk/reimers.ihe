namespace Reimers.Ihe.Http
{
    using System;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Threading.Tasks;

    public class DefaultHttpConnectionFactory
    {
        private readonly Uri _address;
        private readonly Encoding _encoding;
        private readonly X509CertificateCollection _clientCertificateCollection;

        public DefaultHttpConnectionFactory(Uri address, Encoding encoding = null, X509CertificateCollection clientCertificateCollection = null)
        {
            _address = address;
            _encoding = encoding;
            _clientCertificateCollection = clientCertificateCollection;
        }

        public Task<IHostConnection> Get()
        {
            return Task.FromResult<IHostConnection>(new IheHttpClient(_address, _encoding, _clientCertificateCollection));
        }
    }
}