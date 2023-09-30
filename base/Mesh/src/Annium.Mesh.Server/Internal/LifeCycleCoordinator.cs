using System.Collections.Generic;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Mesh.Server.Handlers;
using Annium.Mesh.Server.Models;

namespace Annium.Mesh.Server.Internal;

internal class LifeCycleCoordinator<TState> : ILogSubject
    where TState : ConnectionStateBase
{
    public ILogger Logger { get; }
    private readonly IEnumerable<LifeCycleHandlerBase<TState>> _handlers;

    public LifeCycleCoordinator(
        IEnumerable<LifeCycleHandlerBase<TState>> handlers,
        ILogger logger
    )
    {
        _handlers = handlers;
        Logger = logger;
    }

    public async Task StartAsync(TState state)
    {
        this.Trace("start");

        foreach (var handler in _handlers)
            await handler.StartAsync(state);
    }

    public async Task EndAsync(TState state)
    {
        this.Trace("start");

        foreach (var handler in _handlers)
            await handler.EndAsync(state);
    }
}