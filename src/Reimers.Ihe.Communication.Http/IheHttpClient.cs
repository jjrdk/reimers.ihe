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
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    internal class IheHttpClient : IHostConnection
    {
        private readonly Uri _address;
        private readonly HttpClient _httpClient;
        private readonly Encoding _encoding;

        public IheHttpClient(
            Uri address,
            HttpMessageHandler httpClientHandler,
            Encoding encoding = null)
        {
            _address = address;
            _httpClient = new HttpClient(httpClientHandler, false);
            _encoding = encoding ?? Encoding.ASCII;
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }

        public async Task<Hl7Message> Send(
            string message,
            CancellationToken cancellationToken = default)
        {
            var content = new StreamContent(
                new MemoryStream(_encoding.GetBytes(message)));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/hl7-v2")
            {
                CharSet = _encoding.WebName
            };
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = _address,
                Content = content
            };
            request.Headers.Accept.Add(
                new MediaTypeWithQualityHeaderValue(
                    "application/hl7-v2"));
            request.Headers.Accept.Add(
                new MediaTypeWithQualityHeaderValue(
                    "text/plain"));
            var response = await _httpClient.SendAsync(
                    request,
                    cancellationToken)
                .ConfigureAwait(false);
         await   response.Content.LoadIntoBufferAsync().ConfigureAwait(false);
            var responseContent = await response.Content.ReadAsStringAsync()
                .ConfigureAwait(false);

            return new Hl7Message(responseContent, _address.ToString());
        }
    }
}
