using System;
using Annium.Mesh.Transport.Abstractions;

namespace Annium.Mesh.Transport.InMemory.Internal;

/// <summary>
/// Hub that manages the creation and connection of client and server connections in memory
/// </summary>
internal class ConnectionHub : IConnectionHub
{
    /// <summary>
    /// Event raised when a new server connection is established
    /// </summary>
    public event Action<IServerConnection> OnConnection = delegate { };

    /// <summary>
    /// Creates a new client connection with its corresponding server connection
    /// </summary>
    /// <returns>A new client connection that will trigger server connection creation when connected</returns>
    public IClientConnection Create()
    {
        var channel = new Channel();

        var clientConnection = new ClientConnection(channel);

        channel.OnConnected += () => OnConnection(new ServerConnection(channel));

        return clientConnection;
    }
}
