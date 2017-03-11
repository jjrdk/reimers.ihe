namespace Reimers.Ihe
{
    using System.Security.Cryptography.X509Certificates;
    using System.Threading.Tasks;

    public class DefaultMllpConnectionFactory
    {
        private readonly string _address;
        private readonly int _port;
        private readonly X509CertificateCollection _clientCertificateCollection;

        public DefaultMllpConnectionFactory(string address, int port, X509CertificateCollection clientCertificateCollection = null)
        {
            _address = address;
            _port = port;
            _clientCertificateCollection = clientCertificateCollection;
        }

        public Task<IMllpConnection> Get()
        {
            return MllpClient.Create(_address, _port, _clientCertificateCollection);
        }
    }
}