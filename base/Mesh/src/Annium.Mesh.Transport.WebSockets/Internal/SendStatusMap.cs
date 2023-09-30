using System;
using Annium.Mesh.Transport.Abstractions;
using Annium.Net.WebSockets;

namespace Annium.Mesh.Transport.WebSockets.Internal;

internal static class SendStatusMap
{
    public static SendStatus Map(WebSocketSendStatus status) => status switch
    {
        WebSocketSendStatus.Ok       => SendStatus.Ok,
        WebSocketSendStatus.Canceled => SendStatus.Canceled,
        WebSocketSendStatus.Closed   => SendStatus.Closed,
        _                         => throw new InvalidOperationException($"Unsupported {nameof(WebSocketSendStatus)}: {status}")
    };
}