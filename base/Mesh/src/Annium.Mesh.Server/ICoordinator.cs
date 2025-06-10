using System;
using System.Threading.Tasks;
using Annium.Mesh.Transport.Abstractions;

namespace Annium.Mesh.Server;

/// <summary>
/// Coordinates message handling and connection management for the mesh server,
/// acting as the main entry point for processing incoming connections.
/// </summary>
public interface ICoordinator : IDisposable
{
    /// <summary>
    /// Handles an incoming server connection, managing its lifecycle and message processing.
    /// </summary>
    /// <param name="connection">The server connection to handle.</param>
    /// <returns>A task representing the asynchronous connection handling operation.</returns>
    Task HandleAsync(IServerConnection connection);
}
