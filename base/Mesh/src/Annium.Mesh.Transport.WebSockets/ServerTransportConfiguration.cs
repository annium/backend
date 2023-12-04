using Annium.Net.WebSockets.Internal;

namespace Annium.Mesh.Transport.WebSockets;

public class ServerTransportConfiguration
{
    public ConnectionMonitorOptions ConnectionMonitor { get; init; } = new();
}
