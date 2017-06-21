namespace Reimers.Ihe.Communication
{
    using System;
    using System.Net.Security;
    using System.Security.Authentication;
    using System.Security.Cryptography.X509Certificates;

    /// <summary>
    /// Class ServerSecurityDetails. Defines a DTO to keep the security-related properties of the client-side.
    /// </summary>
    public class ServerSecurityDetails
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServerSecurityDetails"/> class.
        /// </summary>
        /// <param name="serverCertificate">Specifies the server certificate to send to the client for authentication.</param>
        /// <param name="forceClientAuthentication">Indicates if client authentication shall be enforced</param>
        /// <param name="validationCallback">The callback used to validated the client-certificate.</param>
        /// <param name="supportedSslProtocols">Specifies the supported SSL Protocols.</param>
        public ServerSecurityDetails(X509Certificate serverCertificate, bool forceClientAuthentication = false, RemoteCertificateValidationCallback validationCallback = null, SslProtocols supportedSslProtocols = SslProtocols.Tls12)
        {
            this.ServerCertificate = serverCertificate ?? throw new ArgumentNullException(nameof(serverCertificate));
            this.ForceClientAuthentciation = forceClientAuthentication;
            this.ClientCertificateValidationCallback = validationCallback;
            this.SupportedSslProtocols = supportedSslProtocols;
        }

        /// <summary>
        /// The server certificate to be used.
        /// </summary>
        /// <value>The server certificate.</value>
        public X509Certificate ServerCertificate { get; }

        /// <summary>
        /// Indicates if client authentication shall be enforced (Mutual authentication).
        /// </summary>
        public bool ForceClientAuthentciation { get; }

        /// <summary>
        /// In case of mutual authentication: the callback used to validate the client-certificate.
        /// In case of server authentication only: null.
        /// </summary>
        /// <value>Details for client auth. Or null if client is not authenticated.</value>
        public RemoteCertificateValidationCallback ClientCertificateValidationCallback { get; }

        /// <summary>
        /// The supported SSL protocols. The resulting protocols will be negotiated by server and client when the connection is established.
        /// </summary>
        /// <value>The supported SSL protocols.</value>
        public SslProtocols SupportedSslProtocols { get; }
    }
}
