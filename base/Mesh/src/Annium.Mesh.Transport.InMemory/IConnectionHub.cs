using System;
using Annium.Mesh.Transport.Abstractions;

namespace Annium.Mesh.Transport.InMemory;

/// <summary>
/// Hub for managing in-memory connections between clients and servers
/// </summary>
public interface IConnectionHub
{
    /// <summary>
    /// Event triggered when a new server connection is established
    /// </summary>
    event Action<IServerConnection> OnConnection;

    /// <summary>
    /// Creates a new client connection that can connect to this hub
    /// </summary>
    /// <returns>A new client connection instance</returns>
    IClientConnection Create();
}
