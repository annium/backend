using System.Net.WebSockets;

namespace Annium.Net.WebSockets;

internal readonly struct ReceiveResult
{
    public readonly WebSocketMessageType MessageType;
    public readonly int Count;
    public readonly bool EndOfMessage;
    public readonly WebSocketCloseStatus CloseStatus;

    public ReceiveResult(
        WebSocketMessageType messageType,
        int count,
        bool endOfMessage,
        WebSocketCloseStatus closeStatus
    )
    {
        MessageType = messageType;
        Count = count;
        EndOfMessage = endOfMessage;
        CloseStatus = closeStatus;
    }
}