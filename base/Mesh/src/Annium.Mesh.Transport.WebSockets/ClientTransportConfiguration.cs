using System;
using Annium.Net.WebSockets;

namespace Annium.Mesh.Transport.WebSockets;

/// <summary>
/// Configuration for WebSocket-based client transport connections.
/// </summary>
public class ClientTransportConfiguration
{
    /// <summary>
    /// Gets or initializes the WebSocket URI to connect to.
    /// </summary>
    public Uri Uri { get; init; } = default!;

    /// <summary>
    /// Gets or initializes the connection monitoring options.
    /// </summary>
    public ConnectionMonitorOptions ConnectionMonitor { get; init; } = new();

    /// <summary>
    /// Gets or initializes the delay in milliseconds before attempting to reconnect after a connection failure.
    /// </summary>
    public int ReconnectDelay { get; init; }
}
