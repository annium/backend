using System;

namespace Annium.Mesh.Transport.Abstractions;

public interface IClientConnection : ISendingReceivingConnection
{
    event Action OnConnected;
    event Action<ConnectionCloseStatus> OnDisconnected;
    event Action<Exception> OnError;
    void Connect();
    void Disconnect();
}