using System;
using Annium.Mesh.Transport.Abstractions;
using Annium.Net.Sockets;

namespace Annium.Mesh.Transport.Sockets.Internal;

internal static class CloseStatusMap
{
    public static CloseStatus Map(SocketCloseStatus status) => status switch
    {
        SocketCloseStatus.ClosedLocal  => CloseStatus.ClosedLocal,
        SocketCloseStatus.ClosedRemote => CloseStatus.ClosedRemote,
        SocketCloseStatus.Error        => CloseStatus.Error,
        _                              => throw new InvalidOperationException($"Unsupported {nameof(SocketCloseStatus)}: {status}")
    };
}