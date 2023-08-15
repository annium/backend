namespace Annium.Net.WebSockets;

public enum WebSocketReceiveStatus
{
    Normal,
    Canceled,
    ClosedLocal,
    ClosedRemote,
    Error
}