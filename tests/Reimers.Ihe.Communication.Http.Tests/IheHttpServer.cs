// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IheHttpServer.cs" company="Reimers.dk">
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

namespace Reimers.Ihe.Communication.Http.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.TestHost;

    public class IheHttpServer : IDisposable
    {
        private readonly TestServer _server;

        public IheHttpServer(IEnumerable<string> urls, IHl7MessageMiddleware middleware)
        {
            _server = new TestServer(new WebHostBuilder()
                .UseUrls(urls.ToArray())
                .Configure(a =>
                {
                    a.Use(async (ctx, next) =>
                       {
                           using var reader = new StreamReader(ctx.Request.Body);
                           var hl7 = await reader.ReadToEndAsync().ConfigureAwait(false);
                           var response = await middleware.Handle(new Hl7Message(hl7, ctx.Connection.RemoteIpAddress?.ToString()))
                               .ConfigureAwait(false);
                           if (!string.IsNullOrWhiteSpace(response))
                           {
                               var owinResponse = ctx.Response;
                               owinResponse.StatusCode = (int)HttpStatusCode.OK;
                               var charset =
                                   ctx.Request.ContentType?.Contains(';') == true
                                       ? ctx.Request.ContentType
                                           ?.Split(
                                               ';',
                                               StringSplitOptions
                                                   .RemoveEmptyEntries)
                                           .Last()
                                           .Replace(
                                               "charset=",
                                               "",
                                               StringComparison
                                                   .InvariantCultureIgnoreCase)
                                           .Trim()
                                       : null;
                               var encoding = string.IsNullOrWhiteSpace(charset)
                                   ? Encoding.ASCII
                                   : Encoding.GetEncoding(charset);

                               owinResponse.ContentType = "application/hl7-v2; charset=" + encoding.WebName;
                               await owinResponse.BodyWriter.WriteAsync(
                                   encoding.GetBytes(response)
                                       .AsMemory()).ConfigureAwait(false);
                               await owinResponse.CompleteAsync().ConfigureAwait(false);
                           }
                       });
                }));
        }

        public Func<HttpMessageHandler> CreateClientHandler => (() => _server.CreateHandler());

        public void Dispose()
        {
            _server.Dispose();
        }
    }
}