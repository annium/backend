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

    public async Task StartAsync()
    {
        this.Trace("start");

        foreach (var handler in _handlers)
            await handler.StartAsync();

        this.Trace("done");
    }

    public async Task EndAsync()
    {
        this.Trace("start");

        foreach (var handler in _handlers)
            await handler.EndAsync();

        this.Trace("done");
    }
}