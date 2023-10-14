using System;
using Annium.Mesh.Transport.Abstractions;

namespace Annium.Mesh.Transport.InMemory.Internal;

internal class ConnectionHub : IConnectionHub
{
    public event Action<IServerConnection> OnConnection = delegate { };

    public IClientConnection Create()
    {
        var channel = new Channel();

        var clientConnection = new ClientConnection(channel);

        channel.OnConnected += () => OnConnection(new ServerConnection(channel));

        return clientConnection;
    }
}