using System.Security.Cryptography.X509Certificates;
using Annium.Net.Sockets;

namespace Annium.Mesh.Transport.Sockets;

public class ServerTransportConfiguration
{
    public X509Certificate? Certificate { get; init; } = default!;
    public ConnectionMonitorOptions ConnectionMonitor { get; init; } = new();
}
