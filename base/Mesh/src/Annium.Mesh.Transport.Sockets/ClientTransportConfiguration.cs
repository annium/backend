using System;
using System.Net.Security;
using Annium.Net.Sockets;

namespace Annium.Mesh.Transport.Sockets;

public class ClientTransportConfiguration
{
    public Uri Uri { get; init; } = default!;
    public SslClientAuthenticationOptions? AuthOptions { get; init; }
    public ConnectionMonitorOptions ConnectionMonitor { get; init; } = new();
    public int ReconnectDelay { get; init; }
}
