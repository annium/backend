namespace Annium.Mesh.Server.Internal.Routing;

/// <summary>
/// Stores routing information for different types of message handlers in the mesh server.
/// </summary>
internal class RouteStore
{
    /// <summary>
    /// Gets the store for request-response routes.
    /// </summary>
    public RouteTypeStore<RequestRoute> RequestRoutes { get; } = new();

    /// <summary>
    /// Gets the store for push message routes.
    /// </summary>
    public RouteTypeStore<PushRoute> PushRoutes { get; } = new();
}
