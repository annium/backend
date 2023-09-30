using System;

namespace Annium.Mesh.Transport.Abstractions;

public interface IServerConnection : ISendingReceivingConnection
{
    event Action<CloseStatus> OnDisconnected;
    event Action<Exception> OnError;
    void Disconnect();
}