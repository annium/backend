using System;

namespace Annium.Net.WebSockets;

public interface IConnectionMonitor
{
    event Action OnConnectionLost;
    void Start(ISendingReceivingWebSocket socket);
    void Stop();
}