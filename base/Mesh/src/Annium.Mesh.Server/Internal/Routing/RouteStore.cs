namespace Annium.Mesh.Server.Internal.Routing;

internal class RouteStore
{
    public RouteTypeStore<RequestData> RequestRoutes { get; } = new();
}