using System;

namespace Annium.Mesh.Transport.Abstractions;

/// <summary>
/// Represents a client-side connection that can be initiated by the client
/// </summary>
public interface IClientConnection : IManagedConnection
{
    /// <summary>
    /// Event triggered when the connection is successfully established
    /// </summary>
    event Action OnConnected;

    /// <summary>
    /// Initiates the connection to the remote endpoint
    /// </summary>
    void Connect();
}
