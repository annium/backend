using System;

namespace Annium.Net.WebSockets;

public interface IServerWebSocket : ISendingReceivingWebSocket
{
    event Action<WebSocketCloseStatus> OnDisconnected;
    event Action<Exception> OnError;
    void Disconnect();
}