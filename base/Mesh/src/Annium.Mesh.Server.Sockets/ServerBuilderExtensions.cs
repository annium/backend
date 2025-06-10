using Annium.Mesh.Server.Sockets.Internal;
using Annium.Net.Servers.Sockets;

namespace Annium.Mesh.Server.Sockets;

/// <summary>
/// Provides extension methods for configuring socket-based mesh server handlers.
/// </summary>
public static class ServerBuilderExtensions
{
    /// <summary>
    /// Configures the server builder to use the mesh handler for processing socket connections.
    /// </summary>
    /// <param name="builder">The server builder to configure.</param>
    /// <returns>The configured server builder.</returns>
    public static IServerBuilder WithMeshHandler(this IServerBuilder builder)
    {
        return builder.WithHandler<Handler>();
    }
}
