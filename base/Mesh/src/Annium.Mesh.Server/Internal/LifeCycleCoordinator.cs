using System.Collections.Generic;
using System.Threading.Tasks;
using Annium.Logging;

namespace Annium.Mesh.Server.Internal;

/// <summary>
/// Coordinates lifecycle events for all registered lifecycle handlers in a connection scope.
/// </summary>
internal class LifeCycleCoordinator : ILogSubject
{
    /// <summary>
    /// Gets the logger for this lifecycle coordinator.
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// The collection of lifecycle handlers to coordinate.
    /// </summary>
    private readonly IEnumerable<LifeCycleHandlerBase> _handlers;

    /// <summary>
    /// Initializes a new instance of the <see cref="LifeCycleCoordinator"/> class.
    /// </summary>
    /// <param name="handlers">The collection of lifecycle handlers to coordinate.</param>
    /// <param name="logger">The logger for this coordinator.</param>
    public LifeCycleCoordinator(IEnumerable<LifeCycleHandlerBase> handlers, ILogger logger)
    {
        _handlers = handlers;
        Logger = logger;
    }

    /// <summary>
    /// Starts all registered lifecycle handlers asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous start operation.</returns>
    public async Task StartAsync()
    {
        this.Trace("start");

        foreach (var handler in _handlers)
            await handler.StartAsync();

        this.Trace("done");
    }

    /// <summary>
    /// Ends all registered lifecycle handlers asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous end operation.</returns>
    public async Task EndAsync()
    {
        this.Trace("start");

        foreach (var handler in _handlers)
            await handler.EndAsync();

        this.Trace("done");
    }
}
