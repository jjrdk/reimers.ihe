namespace Reimers.Ihe.Communication.Http.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Microsoft.Owin.Hosting;
    using NHapi.Base.Parser;
    using Owin;    public class IheHttpServer : IDisposable
    {
        private readonly PipeParser _parser = new PipeParser();
        private readonly IHl7MessageMiddleware _middleware;
        private readonly IDisposable _server;        public IheHttpServer(IEnumerable<string> urls, IHl7MessageMiddleware middleware)
        {
            _middleware = middleware;
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
                        var response = await _middleware.Handle(new Hl7Message(hl7, ctx.Request.RemoteIpAddress))
                            .ConfigureAwait(false);                        if (!string.IsNullOrWhiteSpace(response))
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
        }        public void Dispose()
        {
            _server.Dispose();
        }
    }
}