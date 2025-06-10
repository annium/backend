using Annium.AspNetCore.Mesh.Internal.Middleware;
using Microsoft.AspNetCore.Builder;

namespace Annium.AspNetCore.Mesh;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseMeshWebSocketsMiddleware(this IApplicationBuilder builder)
    {
        builder.UseWebSockets();
        builder.UseMiddleware<WebSocketsMiddleware>();

        return builder;
    }
}
