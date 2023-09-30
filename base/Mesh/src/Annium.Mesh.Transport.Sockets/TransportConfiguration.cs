using System.Net;
using Annium.Net.Sockets;

namespace Annium.Mesh.Transport.Sockets;

public class TransportConfiguration
{
    public IPEndPoint Endpoint { get; init; } = default!;
    public IConnectionMonitor ConnectionMonitor { get; init; } = Net.Sockets.ConnectionMonitor.None;
    public int ReconnectDelay { get; init; }
}