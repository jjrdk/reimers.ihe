namespace Roche.Connectivity.Mllp.UnitTests
{
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using NHapi.Model.V251.Message;

    using Reimers.Ihe.Communication;
    using Reimers.Ihe.Communication.Tests;


    [TestClass]
    public class MllpServerTests
    {
        private readonly MllpServer testee;

        public MllpServerTests()
        {
            this.testee = new MllpServer(new ServerConnectionDetails(new IPEndPoint(IPAddress.Loopback, 8013)), new TestMiddleware());
            this.testee.Start();
        }

        [TestMethod]
        public void MllpServer_Receives_Message_OML_O33_And_Delegates_It_To_A_Message_Handler_Which_Returns_A_Valid_ORL_O34_Message()
        {
            // Arrange
            OML_O33 omlO33 = this.CreateValidOmlO33Message();
            ORL_O34 orlO34 = this.CreateValidOrlO34Message();
            CancellationTokenSource source = new CancellationTokenSource();

            this.testee.Start();

            // Act
            ClientConnectionDetails connectionDetails = new ClientConnectionDetails(IPAddress.Loopback.ToString(), 8013);
            var client = (MllpClient)MllpClient.Create(connectionDetails).Result;
            var task = client.Send(omlO33.ToString(), source.Token);
            task.Wait(5000, source.Token);

            string returnedValue = task.Result.Message;

            // Assert
            task.Status.Should().Be(TaskStatus.RanToCompletion);
            returnedValue.Should().NotBeNull("we must receive a ORL^O34^ORL_O34 message as response.");
            returnedValue.Should().Be(orlO34.ToString(), "this is the expected content of the response.");
        }

        private ORL_O34 CreateValidOrlO34Message()
        {
            ORL_O34 message = new ORL_O34();
            return message;
        }

        private OML_O33 CreateValidOmlO33Message()
        {
            OML_O33 message = new OML_O33();

            return message;
        }
    }
}
