using System;
using Annium.Mesh.Transport.Abstractions;
using Annium.Net.Sockets;

namespace Annium.Mesh.Transport.Sockets.Internal;

internal static class ConnectionSendStatusMap
{
    public static ConnectionSendStatus Map(SocketSendStatus status) =>
        status switch
        {
            SocketSendStatus.Ok => ConnectionSendStatus.Ok,
            SocketSendStatus.Canceled => ConnectionSendStatus.Canceled,
            SocketSendStatus.Closed => ConnectionSendStatus.Closed,
            _ => throw new InvalidOperationException($"Unsupported {nameof(SocketSendStatus)}: {status}")
        };
}
