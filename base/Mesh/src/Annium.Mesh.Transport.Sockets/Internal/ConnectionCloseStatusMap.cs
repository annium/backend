using System;
using Annium.Mesh.Transport.Abstractions;
using Annium.Net.Sockets;

namespace Annium.Mesh.Transport.Sockets.Internal;

/// <summary>
/// Maps socket close status values to mesh transport connection close status values.
/// </summary>
internal static class ConnectionCloseStatusMap
{
    /// <summary>
    /// Maps a socket close status to a connection close status.
    /// </summary>
    /// <param name="status">The socket close status to map.</param>
    /// <returns>The corresponding connection close status.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the socket close status is not supported.</exception>
    public static ConnectionCloseStatus Map(SocketCloseStatus status) =>
        status switch
        {
            SocketCloseStatus.ClosedLocal => ConnectionCloseStatus.ClosedLocal,
            SocketCloseStatus.ClosedRemote => ConnectionCloseStatus.ClosedRemote,
            SocketCloseStatus.Error => ConnectionCloseStatus.Error,
            _ => throw new InvalidOperationException($"Unsupported {nameof(SocketCloseStatus)}: {status}"),
        };
}
