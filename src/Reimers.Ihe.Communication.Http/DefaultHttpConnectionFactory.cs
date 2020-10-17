// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DefaultHttpConnectionFactory.cs" company="Reimers.dk">
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
    using System.Net.Http;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Threading.Tasks;
    using NHapi.Base.Parser;

    /// <summary>
    /// Defines the default MLLP connection factory.
    /// </summary>
    public class DefaultHttpConnectionFactory : IDisposable
    {
        private readonly Uri _address;
        private readonly PipeParser _parser;
        private readonly Encoding _encoding;
        private readonly HttpMessageHandler _httpClientHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultHttpConnectionFactory"/> class.
        /// </summary>
        /// <param name="address">The address of the remote server.</param>
        /// <param name="parser"></param>
        /// <param name="encoding">The <see cref="Encoding"/> to use for data transfer. If no value is specified, then <see cref="Encoding.ASCII"/> is used.</param>
        /// <param name="clientCertificateCollection">The client certificates to use for connection security.</param>
        /// <param name="httpClientHandlerFactory"></param>
        public DefaultHttpConnectionFactory(
            Uri address,
            PipeParser? parser = null,
            Encoding? encoding = null,
            X509CertificateCollection? clientCertificateCollection = null,
            Func<HttpMessageHandler>? httpClientHandlerFactory = null)
        {
            _address = address;
            _parser = parser ?? new PipeParser();
            _encoding = encoding ?? Encoding.ASCII;
            if (httpClientHandlerFactory != null)
            {
                _httpClientHandler = httpClientHandlerFactory();
            }
            else
            {
                var handler = new HttpClientHandler
                {
                    AllowAutoRedirect = true,
                };
                if (clientCertificateCollection != null)
                {
                    handler.ClientCertificates.AddRange(clientCertificateCollection);
                }
                handler.ClientCertificateOptions = ClientCertificateOption.Manual;
                _httpClientHandler = handler;
            }
        }

        /// <summary>
        /// Gets an instance of an HTTP connection.
        /// </summary>
        /// <returns>An MLLP connection as an async operation.</returns>
        public Task<IHostConnection> Get()
        {
            return Task.FromResult<IHostConnection>(new IheHttpClient(_address, _httpClientHandler, _parser, _encoding));
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _httpClientHandler.Dispose();
        }
    }
}
