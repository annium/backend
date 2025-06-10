using System;
using System.Net.Security;
using Annium.Net.Sockets;

namespace Annium.Mesh.Transport.Sockets;

/// <summary>
/// Configuration for socket-based client transport connections.
/// </summary>
public class ClientTransportConfiguration
{
    /// <summary>
    /// Gets or initializes the URI to connect to.
    /// </summary>
    public Uri Uri { get; init; } = default!;

    /// <summary>
    /// Gets or initializes the SSL client authentication options for secure connections.
    /// </summary>
    public SslClientAuthenticationOptions? AuthOptions { get; init; }

    /// <summary>
    /// Gets or initializes the connection monitoring options.
    /// </summary>
    public ConnectionMonitorOptions ConnectionMonitor { get; init; } = new();

    /// <summary>
    /// Gets or initializes the delay in milliseconds before attempting to reconnect after a connection failure.
    /// </summary>
    public int ReconnectDelay { get; init; }
}
