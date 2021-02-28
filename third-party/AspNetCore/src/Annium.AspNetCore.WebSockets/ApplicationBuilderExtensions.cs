using Annium.AspNetCore.WebSockets.Internal.Middleware;
using Microsoft.AspNetCore.Builder;

namespace Annium.Core.DependencyInjection
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseWebSocketsServer(this IApplicationBuilder builder, string url = "/ws")
        {
            builder.UseWebSockets();
            return builder.Map(url, x => x.UseMiddleware<WebSocketsMiddleware>());
        }
    }
}