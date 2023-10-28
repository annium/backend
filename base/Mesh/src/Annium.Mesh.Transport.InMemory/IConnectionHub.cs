using System;
using Annium.Mesh.Transport.Abstractions;

namespace Annium.Mesh.Transport.InMemory;

public interface IConnectionHub
{
    event Action<IServerConnection> OnConnection;
    IClientConnection Create();
}
