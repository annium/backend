using System;
using Annium.Mesh.Transport.Abstractions;
using Annium.Net.WebSockets;

namespace Annium.Mesh.Transport.WebSockets.Internal;

internal static class ConnectionCloseStatusMap
{
    public static ConnectionCloseStatus Map(WebSocketCloseStatus status) => status switch
    {
        WebSocketCloseStatus.ClosedLocal  => ConnectionCloseStatus.ClosedLocal,
        WebSocketCloseStatus.ClosedRemote => ConnectionCloseStatus.ClosedRemote,
        WebSocketCloseStatus.Error        => ConnectionCloseStatus.Error,
        _                              => throw new InvalidOperationException($"Unsupported {nameof(WebSocketCloseStatus)}: {status}")
    };
}