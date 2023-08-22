using System.Net.WebSockets;

namespace Annium.Net.WebSockets.Internal;

internal readonly struct ReceiveResult
{
    public readonly WebSocketMessageType MessageType;
    public readonly int Count;
    public readonly bool EndOfMessage;
    public readonly WebSocketCloseStatus Status;

    public ReceiveResult(
        WebSocketMessageType messageType,
        int count,
        bool endOfMessage,
        WebSocketCloseStatus status
    )
    {
        MessageType = messageType;
        Count = count;
        EndOfMessage = endOfMessage;
        Status = status;
    }
}