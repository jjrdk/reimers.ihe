namespace Reimers.Ihe.Communication.Http
{
    using System;
    using System.IO;
    using System.Net;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    internal class IheHttpClient : IHostConnection
    {
        private readonly Uri _address;
        private readonly Encoding _encoding;
        private readonly X509CertificateCollection _clientCertificates;

        public IheHttpClient(Uri address, Encoding encoding = null, X509CertificateCollection clientCertificates = null)
        {
            _address = address;
            _encoding = encoding ?? Encoding.ASCII;
            _clientCertificates = clientCertificates ?? new X509Certificate2Collection();
        }

        public void Dispose()
        {
        }

        /// <inheritdoc />
        public event EventHandler<ConnectionStateEventArgs> OnConnectionStateChanged;

        public async Task<Hl7Message> Send(string message,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var request = WebRequest.CreateHttp(_address);
            request.Method = "POST";
            request.ClientCertificates = _clientCertificates;
            request.MediaType = "application/hl7-v2";
            request.Accept = "application/hl7-v2, text/plain";
            request.AllowAutoRedirect = true;
            using (var requestStream = request.GetRequestStream())
            {
                var buffer = _encoding.GetBytes(message);
                await requestStream.WriteAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false);
            }
            var response = request.GetResponse();
            using (var responseStream = response.GetResponseStream())
            {
                using (var reader = new StreamReader(responseStream))
                {
                    var responseContent = await reader.ReadToEndAsync().ConfigureAwait(false);
                    return new Hl7Message(responseContent, _address.ToString());
                }
            }
        }

        /// <inheritdoc />
        public bool IsConnectionEstablished()
        {
            return true;
        }
    }
}
