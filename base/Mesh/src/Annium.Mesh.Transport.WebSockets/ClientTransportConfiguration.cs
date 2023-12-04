using System;
using Annium.Net.WebSockets.Internal;

namespace Annium.Mesh.Transport.WebSockets;

public class ClientTransportConfiguration
{
    public Uri Uri { get; init; } = default!;
    public ConnectionMonitorOptions ConnectionMonitor { get; init; } = new();
    public int ReconnectDelay { get; init; }
}
