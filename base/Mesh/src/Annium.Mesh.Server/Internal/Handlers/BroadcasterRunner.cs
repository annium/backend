using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Mesh.Server.Handlers;
using Annium.Mesh.Server.Internal.Models;

namespace Annium.Mesh.Server.Internal.Handlers;

internal class BroadcasterRunner<TMessage> : IBroadcasterRunner
    where TMessage : notnull
{
    private readonly IBroadcaster<TMessage> _broadcaster;

    public BroadcasterRunner(IBroadcaster<TMessage> broadcaster)
    {
        _broadcaster = broadcaster;
    }

    public Task RunAsync(Action<object> send, CancellationToken ct)
    {
        var ctx = new BroadcastContext<TMessage>(send);

        return _broadcaster.RunAsync(ctx, ct);
    }
}

internal interface IBroadcasterRunner
{
    Task RunAsync(Action<object> send, CancellationToken ct);
}
