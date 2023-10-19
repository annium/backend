namespace Annium.Mesh.Server.Internal.Routing;

internal class RouteStore
{
    public RouteTypeStore<RequestRoute> RequestRoutes { get; } = new();
    public RouteTypeStore<PushRoute> PushRoutes { get; } = new();
}