using Annium.Net.WebSockets;

namespace Annium.Mesh.Transport.WebSockets;

/// <summary>
/// Configuration for WebSocket-based server transport connections.
/// </summary>
public class ServerTransportConfiguration
{
    /// <summary>
    /// Gets or initializes the connection monitoring options.
    /// </summary>
    public ConnectionMonitorOptions ConnectionMonitor { get; init; } = new();
}
