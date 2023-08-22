using System;

namespace Annium.Net.WebSockets;

public interface IClientWebSocket : ISendingReceivingWebSocket
{
    event Action OnConnected;
    event Action<WebSocketCloseStatus> OnDisconnected;
    event Action<Exception> OnError;
    void Connect(Uri uri);
    void Disconnect();
}