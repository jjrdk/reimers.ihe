namespace Reimers.Ihe
{
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Threading.Tasks;

    public class DefaultMllpConnectionFactory
    {
        private readonly string _address;
        private readonly int _port;
        private readonly Encoding _encoding;
        private readonly X509CertificateCollection _clientCertificateCollection;

        public DefaultMllpConnectionFactory(string address, int port, Encoding encoding = null, X509CertificateCollection clientCertificateCollection = null)
        {
            _address = address;
            _port = port;
            _encoding = encoding;
            _clientCertificateCollection = clientCertificateCollection;
        }

        public Task<IMllpConnection> Get()
        {
            return MllpClient.Create(_address, _port, _encoding, _clientCertificateCollection);
        }
    }
}