using System;
using Annium.Net.WebSockets;

namespace Annium.Mesh.Transport.WebSockets;

public class ClientTransportConfiguration
{
    public Uri Uri { get; init; } = default!;
    public IConnectionMonitor ConnectionMonitor { get; init; } = Net.WebSockets.ConnectionMonitor.None;
    public int ReconnectDelay { get; init; }
}
