namespace Roche.Connectivity.Mllp.UnitTests
{
    using System;
    using System.Net;
    using System.Net.Security;
    using System.Security.Authentication;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Threading;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Reimers.Ihe.Communication;
    using Reimers.Ihe.Communication.Tests;

    using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

    [TestClass]
    [DeploymentItem("TestCertificates")]
    public class MllpClientTests
    {
        private static int port = 12000;
        private readonly X509Certificate2 serverCertificate;
        private readonly X509Certificate2 clientCertificate;

        public MllpClientTests()
        {
            this.serverCertificate = LoadCertificate(@"TestCertificates\localhost.pfx");
            this.clientCertificate = LoadCertificate(@"TestCertificates\clientCert.pfx");
        }

        [TestMethod]
        public void MllpHost_Receives_Message_QBP_WOS_Q11_And_Successfully_Replies_With_RSP_WOS_RSP_K11()
        {
            //Arrange
            using (MllpServer server = this.StartupMllpServer())
            {
                var connectionDetails = new ClientConnectionDetails(server.EndPoint.Address.ToString(), server.EndPoint.Port);
                using (MllpClient testee = (MllpClient)MllpClient.Create(connectionDetails).Result)
                {
                    StringBuilder messageToSendBuilder = new StringBuilder();
                    messageToSendBuilder.AppendLine($@"MSH|^~\&|cobas PRIME|LaboratoryName|Host||{DateTime.Now:yyyyMMddHHmmss}||QBP^WOS^QBP_Q11|{Guid.NewGuid()}|P|2.5.1||||||UNICODE UTF-8");
                    messageToSendBuilder.AppendLine(@"SFT|Roche Diagnostics|2.5.36|cobas PRIME|358742.3");
                    messageToSendBuilder.AppendLine(@"SFT|Roche Diagnostics Ltd.|1.1.12|cobas PRIME|554784.2.3");
                    messageToSendBuilder.AppendLine(@"QPD|WOS^Work Order Step^HL70487||$123546|Loaded^sample loaded^99ROC|12345|2A|5^Green^99ROC|200|660|210|100|400|NoSwab^No swab^99ROC|NoSpot^No spot detected^99ROC");
                    messageToSendBuilder.AppendLine(@"RCP|I||R");
                    messageToSendBuilder.AppendLine(@"DSC|156|I");

                    StringBuilder messageToReturnBuilder = new StringBuilder();
                    messageToReturnBuilder.AppendLine($@"MSH|^~\&|Host|LaboratoryName|cobas PRIME|LaboratoryName|{DateTime.Now:yyyyMMddHHmmss}||RSP^WOS^RSP_K11|{Guid.NewGuid()}|P|2.5.1||||||UNICODE UTF-8");
                    messageToReturnBuilder.AppendLine(@"MSA|AA|123456789");
                    messageToReturnBuilder.AppendLine(@"QAK|12|OK|WOS^Work Order Step^IHE_LABTF");
                    messageToReturnBuilder.AppendLine(@"QPD|WOS^Work Order Step^IHE_LABTF");
                    messageToReturnBuilder.AppendLine(@"SPM||$S123456||SER^Serum^HL70487");
                    messageToReturnBuilder.AppendLine(@"OBX|1|CE|TestCode^TestName^LN||74856-6^MPX^LN||||||F");
                    messageToReturnBuilder.AppendLine(@"OBX|2|CE|TestCode^TestName^LN||74857-4^WNV^LN||||||F");
                    messageToReturnBuilder.AppendLine(@"SAC|||$S123456");
                    messageToReturnBuilder.AppendLine(@"ORC|NW|1");

                    //Act
                    var result = testee.Send(messageToSendBuilder.ToString()).GetAwaiter().GetResult();

                    //Assert
                    result.Should().NotBeNull("a message must be returned from the server.");
                    result.Should().Be(messageToReturnBuilder.ToString(), "the receive message must have the expected content");
                }
            }
        }

        [TestMethod]
        public void MllpClient_Receives_Correct_Server_Certificate()
        {
            // arrange
            using (MllpServer server = this.StartupMllpServer(useSsl: true))
            {
                byte[] receivedServerCertificateSerialNumber = null;

                var securityDetails = new ClientSecurityDetails(
                    (sender, certificate, chain, sslPolicyErrors) =>
                    {
                        receivedServerCertificateSerialNumber = certificate.GetSerialNumber();
                        return true;
                    });

                var connectionDetails = new ClientConnectionDetails(server.EndPoint.Address.ToString(), server.EndPoint.Port, Encoding.ASCII, null, securityDetails);

                // act
                using (MllpClient testee = (MllpClient)MllpClient.Create(connectionDetails).Result)
                {

                    // assert
                    Assert.IsNotNull(receivedServerCertificateSerialNumber, "no server certificate was received by client.");
                    CollectionAssert.AreEqual(this.serverCertificate.GetSerialNumber(), receivedServerCertificateSerialNumber, "wrong server certificate received.");
                }
            }
        }

        [TestMethod]
        public void MllpClient_Receives_Server_Certificate_And_Certificate_Rejection_Is_Respected()
        {
            // arrange
            using (MllpServer server = this.StartupMllpServer(useSsl: true))
            {
                var securityDetails = new ClientSecurityDetails((sender, certificate, chain, sslPolicyErrors) => false); // reject server certificate.
                var connectionDetails = new ClientConnectionDetails(server.EndPoint.Address.ToString(), server.EndPoint.Port, Encoding.ASCII, null, securityDetails);

                try
                {
                    // act
                    using (MllpClient testee = (MllpClient)MllpClient.Create(connectionDetails).Result)
                    {
                        // assert
                        Assert.Fail("method should result in error since certificate was rejected.");
                    }
                }
                catch (AggregateException aggregateException)
                {
                    Assert.AreEqual(typeof(AuthenticationException), aggregateException.InnerException.GetType());
                }
            }
        }

        [TestMethod]
        public void MllpClient_Sends_Client_Certificate_Successfully()
        {
            // arrange
            byte[] receivedClientCertificateSerialNumber = null;

            RemoteCertificateValidationCallback clientCertificateValidationCallbackOnServer = (sender, certificate, chain, errors) =>
                {
                    receivedClientCertificateSerialNumber = certificate.GetSerialNumber();

                    // the framework validation of the client cert on the server will result in errors.
                    // accept the client cert anyhow.
                    return true;
                };

            using (MllpServer server = this.StartupMllpServer(true, true, clientCertificateValidationCallbackOnServer))
            {
                // accept any server certificate
                var serverCertificateValidator = new RemoteCertificateValidationCallback((sender, certificate, chain, errors) => true);

                // client certification has issuer-issues. force sending the one we have.
                var clientCertificateCollection = new X509CertificateCollection { this.clientCertificate };
                var clientCertificateSelector = new LocalCertificateSelectionCallback((sender, host, certificates, certificate, issuers) => certificates[0]);

                var securityDetails = new ClientSecurityDetails(serverCertificateValidator, clientCertificateCollection, clientCertificateSelector);

                var connectionDetails = new ClientConnectionDetails(server.EndPoint.Address.ToString(), server.EndPoint.Port, Encoding.ASCII, null, securityDetails);

                // act
                using (MllpClient testee = (MllpClient)MllpClient.Create(connectionDetails).Result)
                {
                }
            }

            // assert
            Assert.IsNotNull(receivedClientCertificateSerialNumber, "no client certificate was received by server.");
            CollectionAssert.AreEqual(this.clientCertificate.GetSerialNumber(), receivedClientCertificateSerialNumber, "wrong server certificate received.");
        }

        private static X509Certificate2 LoadCertificate(string pathToCertificateFile)
        {
            string password = String.Empty;
            return new X509Certificate2(pathToCertificateFile, password);
        }

        private MllpServer StartupMllpServer(bool useSsl = false, bool forceClientAuthentication = false, RemoteCertificateValidationCallback clientCertificateValidationCallback = null)
        {
            string address = IPAddress.Loopback.ToString();
            int uniquePort = Interlocked.Increment(ref port);
            ServerSecurityDetails serverSecurityDetails = useSsl ? new ServerSecurityDetails(serverCertificate, forceClientAuthentication, clientCertificateValidationCallback) : null;
            ServerConnectionDetails serverConnectionDetails = new ServerConnectionDetails(address, uniquePort, null, serverSecurityDetails);

            MllpServer mllpServer = new MllpServer(serverConnectionDetails, new TestMiddleware());
            mllpServer.Start();
            return mllpServer;
        }
    }
}
