using Annium.Mesh.Server.Sockets.Internal;
using Annium.Net.Servers.Sockets;

namespace Annium.Mesh.Server.Sockets;

public static class ServerBuilderExtensions
{
    public static IServerBuilder WithMeshHandler(this IServerBuilder builder)
    {
        return builder.WithHandler<Handler>();
    }
}
