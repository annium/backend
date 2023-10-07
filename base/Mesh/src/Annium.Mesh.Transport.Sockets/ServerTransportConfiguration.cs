using System.Security.Cryptography.X509Certificates;
using Annium.Net.Sockets;

namespace Annium.Mesh.Transport.Sockets;

public class ServerTransportConfiguration
{
    public X509Certificate? Certificate { get; init; } = default!;
    public IConnectionMonitor ConnectionMonitor { get; init; } = Net.Sockets.ConnectionMonitor.None;
}