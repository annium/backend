using System;

namespace Annium.Mesh.Transport.Abstractions;

public interface IManagedConnection : ISendingReceivingConnection
{
    event Action<ConnectionCloseStatus> OnDisconnected;
    event Action<Exception> OnError;
    void Disconnect();
}