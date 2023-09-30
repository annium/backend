using System;
using Annium.Mesh.Transport.Abstractions;
using Annium.Net.WebSockets;

namespace Annium.Mesh.Transport.WebSockets.Internal;

internal static class CloseStatusMap
{
    public static CloseStatus Map(WebSocketCloseStatus status) => status switch
    {
        WebSocketCloseStatus.ClosedLocal  => CloseStatus.ClosedLocal,
        WebSocketCloseStatus.ClosedRemote => CloseStatus.ClosedRemote,
        WebSocketCloseStatus.Error        => CloseStatus.Error,
        _                              => throw new InvalidOperationException($"Unsupported {nameof(WebSocketCloseStatus)}: {status}")
    };
}