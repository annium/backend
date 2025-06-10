using System;
using Annium.Mesh.Transport.Abstractions;
using Annium.Net.WebSockets;

namespace Annium.Mesh.Transport.WebSockets.Internal;

/// <summary>
/// Maps WebSocket send status values to mesh transport connection send status values.
/// </summary>
internal static class ConnectionSendStatusMap
{
    /// <summary>
    /// Maps a WebSocket send status to a connection send status.
    /// </summary>
    /// <param name="status">The WebSocket send status to map.</param>
    /// <returns>The corresponding connection send status.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the WebSocket send status is not supported.</exception>
    public static ConnectionSendStatus Map(WebSocketSendStatus status) =>
        status switch
        {
            WebSocketSendStatus.Ok => ConnectionSendStatus.Ok,
            WebSocketSendStatus.Canceled => ConnectionSendStatus.Canceled,
            WebSocketSendStatus.Closed => ConnectionSendStatus.Closed,
            _ => throw new InvalidOperationException($"Unsupported {nameof(WebSocketSendStatus)}: {status}"),
        };
}
