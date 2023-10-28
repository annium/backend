using System;
using Annium.Mesh.Transport.Abstractions;

namespace Annium.Mesh.Client;

public interface IClient : IClientBase, IAsyncDisposable
{
    event Action OnConnected;
    event Action<ConnectionCloseStatus> OnDisconnected;
    event Action<Exception> OnError;
    void Connect();
    void Disconnect();
}
