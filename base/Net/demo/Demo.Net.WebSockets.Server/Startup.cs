using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

namespace Demo.Net.WebSockets.Server;

public class Startup
{
    public void Configure(IApplicationBuilder app, IHostEnvironment env)
    {
        app.UseWebSockets();

        app.UseMiddleware<WebSocketEchoMiddleware>();

        app.UseRouting();
    }
}