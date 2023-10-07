using System;
using Annium.Mesh.Transport.Abstractions;
using Annium.Net.WebSockets;

namespace Annium.Mesh.Transport.WebSockets.Internal;

internal static class ConnectionSendStatusMap
{
    public static ConnectionSendStatus Map(WebSocketSendStatus status) => status switch
    {
        WebSocketSendStatus.Ok       => ConnectionSendStatus.Ok,
        WebSocketSendStatus.Canceled => ConnectionSendStatus.Canceled,
        WebSocketSendStatus.Closed   => ConnectionSendStatus.Closed,
        _                         => throw new InvalidOperationException($"Unsupported {nameof(WebSocketSendStatus)}: {status}")
    };
}