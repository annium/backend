using System;
using Annium.Core.DependencyInjection;
using Annium.Infrastructure.WebSockets.Domain;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

namespace Demo.Infrastructure.WebSockets.Server
{
    public class Startup
    {
        public void Configure(IApplicationBuilder app, IHostEnvironment env)
        {
            var configuration = app.ApplicationServices.Resolve<Configuration>();
            Console.WriteLine(configuration.UseText ? "text" : "binary");
            app.UseWebSocketsServer(cfg =>
                cfg.UseFormat(configuration.UseText ? SerializationFormat.Text : SerializationFormat.Binary)
            );
        }
    }
}