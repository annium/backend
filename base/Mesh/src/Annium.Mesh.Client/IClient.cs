using System;
using Annium.Mesh.Transport.Abstractions;

namespace Annium.Mesh.Client;

/// <summary>
/// Mesh client interface that provides connection management and communication capabilities
/// </summary>
public interface IClient : IClientBase, IAsyncDisposable
{
    /// <summary>
    /// Event fired when the client successfully connects to the server
    /// </summary>
    event Action OnConnected;

    /// <summary>
    /// Event fired when the client disconnects from the server
    /// </summary>
    event Action<ConnectionCloseStatus> OnDisconnected;

    /// <summary>
    /// Event fired when an error occurs during client operations
    /// </summary>
    event Action<Exception> OnError;

    /// <summary>
    /// Establishes a connection to the server
    /// </summary>
    void Connect();

    /// <summary>
    /// Closes the connection to the server
    /// </summary>
    void Disconnect();
}
