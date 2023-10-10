using Annium.Mesh.Server.Web.Internal;
using Annium.Net.Servers.Web;

namespace Annium.Mesh.Server.Web;

public static class ServerBuilderExtensions
{
    public static IServerBuilder WithMeshHandler(this IServerBuilder builder)
    {
        return builder.WithWebSocketHandler<Handler>();
    }
}