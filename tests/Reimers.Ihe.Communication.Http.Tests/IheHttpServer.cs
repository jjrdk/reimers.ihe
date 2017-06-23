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
    using Microsoft.Owin.Hosting;
    using Owin;
    public class IheHttpServer : IDisposable
    {
        private readonly IDisposable _server;

        public IheHttpServer(IEnumerable<string> urls, IHl7MessageMiddleware middleware)
        {
            var startOptions = new StartOptions();
            foreach (var url in urls)
            {
                startOptions.Urls.Add(url);
            }

            _server = WebApp.Start(startOptions, a =>
            {
                a.Run(async ctx =>
                {
                    using (var reader = new StreamReader(ctx.Request.Body))
                    {
                        var hl7 = await reader.ReadToEndAsync().ConfigureAwait(false);
                        var response = await middleware.Handle(new Hl7Message(hl7, ctx.Request.RemoteIpAddress))
                            .ConfigureAwait(false); if (!string.IsNullOrWhiteSpace(response))
                        {
                            var owinResponse = ctx.Response;
                            owinResponse.ContentType = "application/hl7-v2";
                            owinResponse.StatusCode = 200;
                            using (var writer = new StreamWriter(owinResponse.Body))
                            {
                                await writer.WriteAsync(response).ConfigureAwait(false);
                            }
                        }
                    }
                });
            });
        }
        public void Dispose()
        {
            _server.Dispose();
        }
    }
}