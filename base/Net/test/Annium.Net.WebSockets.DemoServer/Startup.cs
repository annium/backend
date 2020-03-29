using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

namespace Annium.Net.WebSockets.DemoServer
{
    public class Startup
    {
        public void Configure(IApplicationBuilder app, IHostEnvironment env)
        {
            app.UseWebSockets();

            app.UseMiddleware<WebSocketEchoMiddleware>();

            app.UseRouting();
        }
    }
}