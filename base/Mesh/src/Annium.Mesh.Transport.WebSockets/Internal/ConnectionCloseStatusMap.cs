using System;
using Annium.Mesh.Transport.Abstractions;
using Annium.Net.WebSockets;

namespace Annium.Mesh.Transport.WebSockets.Internal;

/// <summary>
/// Maps WebSocket close status values to mesh transport connection close status values.
/// </summary>
internal static class ConnectionCloseStatusMap
{
    /// <summary>
    /// Maps a WebSocket close status to a connection close status.
    /// </summary>
    /// <param name="status">The WebSocket close status to map.</param>
    /// <returns>The corresponding connection close status.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the WebSocket close status is not supported.</exception>
    public static ConnectionCloseStatus Map(WebSocketCloseStatus status) =>
        status switch
        {
            WebSocketCloseStatus.ClosedLocal => ConnectionCloseStatus.ClosedLocal,
            WebSocketCloseStatus.ClosedRemote => ConnectionCloseStatus.ClosedRemote,
            WebSocketCloseStatus.Error => ConnectionCloseStatus.Error,
            _ => throw new InvalidOperationException($"Unsupported {nameof(WebSocketCloseStatus)}: {status}"),
        };
}
