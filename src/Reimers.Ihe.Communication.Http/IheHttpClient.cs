// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IheHttpClient.cs" company="Reimers.dk">
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

        public async Task<Hl7Message> Send(string message, CancellationToken cancellationToken = default(CancellationToken))
        {
            var request = WebRequest.CreateHttp(_address);
            request.Method = "POST";
            request.ClientCertificates = _clientCertificates;
            request.MediaType = "application/hl7-v2";
            request.Accept = "application/hl7-v2, text/plain";
            request.AllowAutoRedirect = true;
            using (var requestStream = await request.GetRequestStreamAsync())
            {
                var buffer = _encoding.GetBytes(message);
                await requestStream.WriteAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false);
            }
            var response = await request.GetResponseAsync();
            using (var responseStream = response.GetResponseStream())
            {
                using (var reader = new StreamReader(responseStream))
                {
                    var responseContent = await reader.ReadToEndAsync().ConfigureAwait(false);
                    return new Hl7Message(responseContent, _address.ToString());
                }
            }
        }
    }
}
