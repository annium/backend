using System.Security.Cryptography.X509Certificates;
using Annium.Net.Sockets;

namespace Annium.Mesh.Transport.Sockets;

/// <summary>
/// Configuration for socket-based server transport connections.
/// </summary>
public class ServerTransportConfiguration
{
    /// <summary>
    /// Gets or initializes the X.509 certificate for SSL/TLS secured connections.
    /// </summary>
    public X509Certificate? Certificate { get; init; } = default!;

    /// <summary>
    /// Gets or initializes the connection monitoring options.
    /// </summary>
    public ConnectionMonitorOptions ConnectionMonitor { get; init; } = new();
}
