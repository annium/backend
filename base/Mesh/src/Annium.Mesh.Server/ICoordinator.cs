using System;
using System.Threading;
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
    /// <param name="ct">The transport/server cancellation token; when it fires (e.g. the server is
    /// shutting down), the connection and its push handlers are cancelled so the server's connection
    /// drain can complete instead of waiting on handlers that never stop.</param>
    /// <returns>A task representing the asynchronous connection handling operation.</returns>
    Task HandleAsync(IServerConnection connection, CancellationToken ct);
}
