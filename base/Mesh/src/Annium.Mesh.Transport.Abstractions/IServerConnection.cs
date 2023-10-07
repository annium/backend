using System;

namespace Annium.Mesh.Transport.Abstractions;

public interface IServerConnection : ISendingReceivingConnection
{
    Guid Id { get; }
    event Action<ConnectionCloseStatus> OnDisconnected;
    event Action<Exception> OnError;
    void Disconnect();
}