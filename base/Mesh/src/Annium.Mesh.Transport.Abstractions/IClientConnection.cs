using System;

namespace Annium.Mesh.Transport.Abstractions;

public interface IClientConnection : IManagedConnection
{
    event Action OnConnected;
    void Connect();
}