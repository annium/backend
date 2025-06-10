using Annium.Mesh.Server.Web.Internal;
using Annium.Net.Servers.Web;

namespace Annium.Mesh.Server.Web;

/// <summary>
/// Provides extension methods for configuring web-based mesh server handlers.
/// </summary>
public static class ServerBuilderExtensions
{
    /// <summary>
    /// Configures the server builder to use the mesh handler for processing WebSocket connections.
    /// </summary>
    /// <param name="builder">The server builder to configure.</param>
    /// <returns>The configured server builder.</returns>
    public static IServerBuilder WithMeshHandler(this IServerBuilder builder)
    {
        return builder.WithWebSocketHandler<Handler>();
    }
}
