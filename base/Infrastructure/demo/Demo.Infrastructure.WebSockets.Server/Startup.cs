using Annium.Core.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

namespace Demo.Infrastructure.WebSockets.Server;

public class Startup
{
    public void Configure(IApplicationBuilder app, IHostEnvironment env)
    {
        app.UseWebSocketsServer();
    }
}