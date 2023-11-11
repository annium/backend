using System;
using System.Net.Security;
using Annium.Net.Sockets;

namespace Annium.Mesh.Transport.Sockets;

public class ClientTransportConfiguration
{
    public Uri Uri { get; init; } = default!;
    public SslClientAuthenticationOptions? AuthOptions { get; init; }
    public IConnectionMonitor ConnectionMonitor { get; init; } = Net.Sockets.ConnectionMonitor.None;
    public int ReconnectDelay { get; init; }
}
