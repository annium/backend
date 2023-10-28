using Annium.Net.WebSockets;

namespace Annium.Mesh.Transport.WebSockets;

public class ServerTransportConfiguration
{
    public IConnectionMonitor ConnectionMonitor { get; init; } = Net.WebSockets.ConnectionMonitor.None;
}
