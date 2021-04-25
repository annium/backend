using Annium.Architecture.Http.Profiles;
using Annium.AspNetCore.WebSockets.Internal.Middleware;
using Annium.Core.Runtime;
using Microsoft.AspNetCore.Builder;

[assembly: ReferTypeAssembly(typeof(HttpStatusCodeProfile))]

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