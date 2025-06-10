using Annium.AspNetCore.Mesh.Internal.Middleware;
using Microsoft.AspNetCore.Builder;

namespace Annium.AspNetCore.Mesh;

/// <summary>
/// Extension methods for configuring Mesh WebSockets middleware in the application pipeline
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Adds Mesh WebSockets middleware to the application pipeline
    /// </summary>
    /// <param name="builder">The application builder to configure</param>
    /// <returns>The configured application builder</returns>
    public static IApplicationBuilder UseMeshWebSocketsMiddleware(this IApplicationBuilder builder)
    {
        builder.UseWebSockets();
        builder.UseMiddleware<WebSocketsMiddleware>();

        return builder;
    }
}
