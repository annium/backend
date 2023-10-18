using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Mesh.Server.Handlers;
using Annium.Mesh.Server.Internal.Models;

namespace Annium.Mesh.Server.Internal.Handlers;

internal class BroadcasterRunner<TMessage> : IBroadcasterRunner
{
    private readonly IBroadcaster<TMessage> _broadcaster;

    public BroadcasterRunner(
        IBroadcaster<TMessage> broadcaster
    )
    {
        _broadcaster = broadcaster;
    }

    public Task Run(Action<object> send, CancellationToken ct)
    {
        var ctx = new BroadcastContext<TMessage>(send);

        return _broadcaster.Run(ctx, ct);
    }
}

internal interface IBroadcasterRunner
{
    Task Run(Action<object> send, CancellationToken ct);
}