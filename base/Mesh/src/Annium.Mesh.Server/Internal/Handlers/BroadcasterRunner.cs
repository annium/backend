using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Mesh.Server.Internal.Models;

namespace Annium.Mesh.Server.Internal.Handlers;

/// <summary>
/// Provides a generic wrapper for running broadcasters, adapting them to the internal broadcasting infrastructure.
/// </summary>
/// <typeparam name="TMessage">The type of messages handled by the broadcaster.</typeparam>
internal class BroadcasterRunner<TMessage> : IBroadcasterRunner
    where TMessage : notnull
{
    /// <summary>
    /// The broadcaster instance that handles the actual broadcasting logic.
    /// </summary>
    private readonly IBroadcaster<TMessage> _broadcaster;

    /// <summary>
    /// Initializes a new instance of the <see cref="BroadcasterRunner{TMessage}"/> class.
    /// </summary>
    /// <param name="broadcaster">The broadcaster to wrap and execute.</param>
    public BroadcasterRunner(IBroadcaster<TMessage> broadcaster)
    {
        _broadcaster = broadcaster;
    }

    /// <summary>
    /// Runs the broadcaster with the specified send action and cancellation token.
    /// </summary>
    /// <param name="send">The action to invoke when broadcasting messages.</param>
    /// <param name="ct">The cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous broadcast operation.</returns>
    public Task RunAsync(Action<object> send, CancellationToken ct)
    {
        var ctx = new BroadcastContext<TMessage>(send);

        return _broadcaster.RunAsync(ctx, ct);
    }
}

/// <summary>
/// Defines a contract for running broadcasters in the mesh server infrastructure.
/// </summary>
internal interface IBroadcasterRunner
{
    /// <summary>
    /// Runs the broadcaster with the specified send action and cancellation token.
    /// </summary>
    /// <param name="send">The action to invoke when broadcasting messages.</param>
    /// <param name="ct">The cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous broadcast operation.</returns>
    Task RunAsync(Action<object> send, CancellationToken ct);
}
