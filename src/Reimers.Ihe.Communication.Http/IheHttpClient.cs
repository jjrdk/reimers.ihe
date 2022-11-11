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
    using Abstractions;
    using NHapi.Base.Model;
    using NHapi.Base.Parser;

    internal class IheHttpClient : IHostConnection
    {
        private readonly Uri _address;
        private readonly PipeParser _parser;
        private readonly HttpClient _httpClient;
        private readonly Encoding _encoding;

        public IheHttpClient(
            Uri address,
            HttpMessageHandler httpClientHandler,
            PipeParser? parser = null,
            Encoding? encoding = null)
        {
            _address = address;
            _parser = parser ?? new PipeParser();
            _httpClient = new HttpClient(httpClientHandler, false);
            _encoding = encoding ?? Encoding.ASCII;
        }

        public ValueTask DisposeAsync()
        {
            _httpClient.Dispose();
            GC.SuppressFinalize(this);
            return new ValueTask();
        }

        public async Task<Hl7Message> Send<TMessage>(
            TMessage message,
            CancellationToken cancellationToken = default)
        where TMessage : IMessage
        {
            var hl7 = _parser.Encode(message);
            var content = new StreamContent(
                new MemoryStream(_encoding.GetBytes(hl7)));
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
            await response.Content.LoadIntoBufferAsync().ConfigureAwait(false);
            var responseContent = await response.Content.ReadAsStringAsync()
                .ConfigureAwait(false);

            var msg = _parser.Parse(responseContent);
            return new Hl7Message(msg, _address.ToString());
        }
    }
}
