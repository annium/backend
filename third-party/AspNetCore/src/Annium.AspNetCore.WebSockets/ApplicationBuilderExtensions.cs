using Annium.AspNetCore.WebSockets.Internal.Middleware;
using Microsoft.AspNetCore.Builder;

namespace Annium.Core.DependencyInjection
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseWebSocketsServer(
            this IApplicationBuilder builder,
            string endpoint = "/ws"
        )
        {
            builder.UseWebSockets();

            return builder.Map(endpoint, x => x.UseMiddleware<WebSocketsMiddleware>());
        }
    }
}