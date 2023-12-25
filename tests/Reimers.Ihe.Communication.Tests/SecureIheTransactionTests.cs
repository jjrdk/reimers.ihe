// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SecureIheTransactionTests.cs" company="Reimers.dk">
//   Copyright © Reimers.dk 2017
//   This source is subject to the MIT License.
//   Please see https://opensource.org/licenses/MIT for details.
//   All other rights reserved.
//
//   The above copyright notice and this permission notice shall be included in
//   all copies or substantial portions of the Software.
//
//   THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//   IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//   FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//   AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//   LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//   OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//   THE SOFTWARE.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Reimers.Ihe.Communication.Tests
{
    using System;
    using System.Net;
    using System.Net.Security;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading.Tasks;
    using NHapi.Model.V251.Message;
    using Xunit;

    public class SecureIheTransactionTests : IAsyncDisposable
    {
        private readonly MllpServer _server;
        private const int Port = 2576;
        private readonly X509Certificate2Collection _cert;

        public SecureIheTransactionTests()
        {
            _cert = new X509Certificate2Collection(
                new X509Certificate2("cert.pfx", "password"));
            _server = new MllpServer(
                new IPEndPoint(IPAddress.Loopback, Port),
                NullLog.Get(),
                new TestMiddleware(),
                serverCertificate: _cert[0],
                userCertificateValidationCallback: UserCertificateValidationCallback);
            _server.Start();
        }

        private static bool UserCertificateValidationCallback(
            object sender,
            X509Certificate? certificate,
            X509Chain? chain,
            SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        [Fact]
        public async Task WhenSendingMessageThenGetsAck()
        {
            var client = await MllpClient.Create(
                IPAddress.Loopback.ToString(),
                Port,
                clientCertificates: _cert,
                userCertificateValidationCallback:
                UserCertificateValidationCallback);
            var request = new QBP_Q11();
            request.MSH.MessageControlID.Value = "test";
            var response = await client.Send(request);
            Assert.NotNull(response);
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            GC.SuppressFinalize(this);
            await _server.DisposeAsync();
        }
    }
}
