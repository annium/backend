using System;
using Annium.Mesh.Transport.Abstractions;
using Annium.Net.Sockets;

namespace Annium.Mesh.Transport.Sockets.Internal;

/// <summary>
/// Maps socket send status values to mesh transport connection send status values.
/// </summary>
internal static class ConnectionSendStatusMap
{
    /// <summary>
    /// Maps a socket send status to a connection send status.
    /// </summary>
    /// <param name="status">The socket send status to map.</param>
    /// <returns>The corresponding connection send status.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the socket send status is not supported.</exception>
    public static ConnectionSendStatus Map(SocketSendStatus status) =>
        status switch
        {
            SocketSendStatus.Ok => ConnectionSendStatus.Ok,
            SocketSendStatus.Canceled => ConnectionSendStatus.Canceled,
            SocketSendStatus.Closed => ConnectionSendStatus.Closed,
            _ => throw new InvalidOperationException($"Unsupported {nameof(SocketSendStatus)}: {status}"),
        };
}
