using Annium.AspNetCore.Mesh.Internal.Middleware;
using Microsoft.AspNetCore.Builder;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseMeshWebSocketsMiddleware(
        this IApplicationBuilder builder
    )
    {
        builder.UseWebSockets();
        builder.UseMiddleware<WebSocketsMiddleware>();

        return builder;
    }
}