using System.Threading;
using System.Threading.Tasks;
using Annium.Net.WebSockets;

namespace Annium.Mesh.Server.Internal.Serialization;

internal static class SendingWebSocketExtensions
{
    public static ValueTask<WebSocketSendStatus> SendWith<T>(
        this ISendingWebSocket socket,
        T value,
        Serializer serializer,
        CancellationToken ct
    )
    {
        return socket.SendBinaryAsync(serializer.Serialize(value), ct);
    }
}