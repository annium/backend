using System;

namespace Annium.Mesh.Transport.Abstractions;

/// <summary>
/// Represents a managed connection that supports both sending and receiving data with lifecycle management
/// </summary>
public interface IManagedConnection : ISendingReceivingConnection
{
    /// <summary>
    /// Event triggered when the connection is disconnected
    /// </summary>
    event Action<ConnectionCloseStatus> OnDisconnected;

    /// <summary>
    /// Event triggered when an error occurs on the connection
    /// </summary>
    event Action<Exception> OnError;

    /// <summary>
    /// Disconnects the connection
    /// </summary>
    void Disconnect();
}
