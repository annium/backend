using System.Net.WebSockets;

namespace Annium.Net.WebSockets.Internal;

internal readonly struct ReceiveResult
{
    public readonly WebSocketMessageType MessageType;
    public readonly int Count;
    public readonly bool EndOfMessage;
    public readonly WebSocketReceiveStatus Status;

    public ReceiveResult(
        WebSocketMessageType messageType,
        int count,
        bool endOfMessage,
        WebSocketReceiveStatus status
    )
    {
        MessageType = messageType;
        Count = count;
        EndOfMessage = endOfMessage;
        Status = status;
    }
}