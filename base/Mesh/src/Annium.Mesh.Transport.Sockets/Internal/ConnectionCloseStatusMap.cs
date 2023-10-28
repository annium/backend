using System;
using Annium.Mesh.Transport.Abstractions;
using Annium.Net.Sockets;

namespace Annium.Mesh.Transport.Sockets.Internal;

internal static class ConnectionCloseStatusMap
{
    public static ConnectionCloseStatus Map(SocketCloseStatus status) =>
        status switch
        {
            SocketCloseStatus.ClosedLocal => ConnectionCloseStatus.ClosedLocal,
            SocketCloseStatus.ClosedRemote => ConnectionCloseStatus.ClosedRemote,
            SocketCloseStatus.Error => ConnectionCloseStatus.Error,
            _ => throw new InvalidOperationException($"Unsupported {nameof(SocketCloseStatus)}: {status}")
        };
}
