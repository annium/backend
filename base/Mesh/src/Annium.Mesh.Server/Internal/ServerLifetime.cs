using System.Threading;

namespace Annium.Mesh.Server.Internal;

/// <summary>
/// Manages the lifetime of the mesh server, providing cancellation tokens and stop functionality.
/// </summary>
internal class ServerLifetime : IServerLifetimeManager, IServerLifetime
{
    /// <summary>
    /// Gets a cancellation token that is canceled when the server is stopping.
    /// </summary>
    public CancellationToken Stopping => _stoppingCts.Token;

    /// <summary>
    /// The cancellation token source used to signal when the server is stopping.
    /// </summary>
    private readonly CancellationTokenSource _stoppingCts = new();

    /// <summary>
    /// Stops the server by canceling the stopping token.
    /// </summary>
    public void Stop() => _stoppingCts.Cancel();
}

/// <summary>
/// Defines management operations for the server lifetime.
/// </summary>
internal interface IServerLifetimeManager
{
    /// <summary>
    /// Gets a cancellation token that is canceled when the server is stopping.
    /// </summary>
    CancellationToken Stopping { get; }

    /// <summary>
    /// Stops the server.
    /// </summary>
    void Stop();
}

/// <summary>
/// Provides read-only access to the server lifetime state.
/// </summary>
internal interface IServerLifetime
{
    /// <summary>
    /// Gets a cancellation token that is canceled when the server is stopping.
    /// </summary>
    CancellationToken Stopping { get; }
}
