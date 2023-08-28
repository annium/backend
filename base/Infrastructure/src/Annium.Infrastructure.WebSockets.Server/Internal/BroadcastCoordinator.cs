using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Infrastructure.WebSockets.Server.Internal.Handlers;
using Annium.Infrastructure.WebSockets.Server.Internal.Serialization;
using Annium.Logging;

namespace Annium.Infrastructure.WebSockets.Server.Internal;

internal class BroadcastCoordinator : ILogSubject, IAsyncDisposable
{
    public ILogger Logger { get; }
    private readonly ConnectionTracker _connectionTracker;
    private readonly IEnumerable<IBroadcasterRunner> _runners;
    private readonly Serializer _serializer;
    private readonly CancellationTokenSource _lifetimeCts;
    private Task _runnersTask = Task.CompletedTask;

    public BroadcastCoordinator(
        ConnectionTracker connectionTracker,
        IEnumerable<IBroadcasterRunner> runners,
        Serializer serializer,
        ILogger logger
    )
    {
        Logger = logger;
        _connectionTracker = connectionTracker;
        _runners = runners;
        _serializer = serializer;
        _lifetimeCts = new CancellationTokenSource();
    }

    public void Start()
    {
        Task.Run(async () =>
        {
            try
            {
                await (_runnersTask = Task.WhenAll(_runners.Select(
                    x => x.Run(Broadcast, _lifetimeCts.Token)
                )));
            }
            catch (Exception e)
            {
                if (e is not TaskCanceledException)
                    this.Log().Error(e);
            }
        });
    }

    public async ValueTask DisposeAsync()
    {
        _lifetimeCts.Cancel();
        try
        {
            await _runnersTask;
        }
        catch (Exception e)
        {
            if (e is not TaskCanceledException)
                this.Log().Error(e);
        }
    }

    private void Broadcast(object message)
    {
        var connections = _connectionTracker.Slice();
        if (connections.Count == 0)
            return;
        var data = _serializer.Serialize(message);
        Task.WhenAll(connections.Select(async x => await x.Socket.SendSerialized(data, CancellationToken.None)));
    }
}