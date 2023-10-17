using System.Collections.Generic;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Mesh.Server.Handlers;
using Annium.Mesh.Server.Internal.Models;

namespace Annium.Mesh.Server.Internal;

internal class LifeCycleCoordinator : ILogSubject
{
    public ILogger Logger { get; }
    private readonly IEnumerable<LifeCycleHandlerBase> _handlers;

    public LifeCycleCoordinator(
        IEnumerable<LifeCycleHandlerBase> handlers,
        ILogger logger
    )
    {
        _handlers = handlers;
        Logger = logger;
    }

    public async Task StartAsync(ConnectionState state)
    {
        this.Trace("start");

        foreach (var handler in _handlers)
            await handler.StartAsync(state);
    }

    public async Task EndAsync(ConnectionState state)
    {
        this.Trace("start");

        foreach (var handler in _handlers)
            await handler.EndAsync(state);
    }
}