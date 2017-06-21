namespace Reimers.Ihe.Communication
{
    using System;
    using System.Net.Security;
    using System.Security.Authentication;
    using System.Security.Cryptography.X509Certificates;

    /// <summary>
    /// Class ClientSecurityDetails. Defines a DTO to keep the security-related properties of the client-side.
    /// </summary> 
    public class ClientSecurityDetails
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClientSecurityDetails"/> class.
        /// </summary>
        /// <param name="serverCertificateValidationCallback">Specifies the callback to validate the server-certificate received from the server.</param>
        /// <param name="clientCertificates">Specifies the client-certificates sent to the server for authentication if authentication is requested by the server.</param>
        /// <param name="clientCertificateSelectionCallback"></param>
        /// <param name="supportedSslProtocols">Specifies the supported SSL protocols. The final protocol will be negotiated by server and client.</param>
        public ClientSecurityDetails(RemoteCertificateValidationCallback serverCertificateValidationCallback, X509CertificateCollection clientCertificates = null, LocalCertificateSelectionCallback clientCertificateSelectionCallback = null, SslProtocols supportedSslProtocols = SslProtocols.Tls12)
        {
            this.ServerCertificateValidationCallback = serverCertificateValidationCallback ?? throw new ArgumentNullException(nameof(serverCertificateValidationCallback));
            this.ClientCertificates = clientCertificates;
            this.ClientCertificateSelectionCallback = clientCertificateSelectionCallback;
            this.SupportedSslProtocols = supportedSslProtocols;
        }

        /// <summary>
        /// The callback to be executed to validate the certificate presented by the server.
        /// </summary>
        /// <value>Callback for validating server certificate.</value>
        public RemoteCertificateValidationCallback ServerCertificateValidationCallback { get; }

        /// <summary>
        /// The client certificates from which one is chose to be sent to the server.
        /// </summary>
        /// <value>The client certificates from which one is chosen to be sent to the server.</value>
        public X509CertificateCollection ClientCertificates { get; }

        /// <summary>
        /// The callback being called to select the client certificate to send to the server.
        /// </summary>
        /// <value>The callback being called to select the client certificate.</value>
        public LocalCertificateSelectionCallback ClientCertificateSelectionCallback { get; }
        /// <summary>
        /// Gets the SSL protocols which are acceptable for the client.
        /// </summary>
        /// <value>The supported SSL protocols.</value>
        public SslProtocols SupportedSslProtocols { get; }
    }
}
