using Annium.Net.WebSockets;

namespace Annium.Mesh.Transport.WebSockets;

public class ServerTransportConfiguration
{
    public ConnectionMonitorOptions ConnectionMonitor { get; init; } = new();
}
