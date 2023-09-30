using System;
using Annium.Mesh.Transport.Abstractions;
using Annium.Net.Sockets;

namespace Annium.Mesh.Transport.Sockets.Internal;

internal static class SendStatusMap
{
    public static SendStatus Map(SocketSendStatus status) => status switch
    {
        SocketSendStatus.Ok       => SendStatus.Ok,
        SocketSendStatus.Canceled => SendStatus.Canceled,
        SocketSendStatus.Closed   => SendStatus.Closed,
        _                         => throw new InvalidOperationException($"Unsupported {nameof(SocketSendStatus)}: {status}")
    };
}