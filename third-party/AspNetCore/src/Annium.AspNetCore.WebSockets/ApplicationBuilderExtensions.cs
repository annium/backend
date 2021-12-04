using Annium.AspNetCore.WebSockets.Internal.Middleware;
using Microsoft.AspNetCore.Builder;

namespace Annium.Core.DependencyInjection;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseWebSocketsServer(
        this IApplicationBuilder builder
    )
    {
        builder.UseWebSockets();
        builder.UseMiddleware<WebSocketsMiddleware>();

        return builder;
    }
}