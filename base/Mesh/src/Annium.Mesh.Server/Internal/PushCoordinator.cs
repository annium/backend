using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Logging;
using Annium.Mesh.Server.Components;
using Annium.Mesh.Server.Internal.Models;
using Annium.Mesh.Server.Internal.Routing;
using Annium.Mesh.Transport.Abstractions;

namespace Annium.Mesh.Server.Internal;

/// <summary>
/// Coordinates the execution of push handlers for established connections
/// </summary>
internal class PushCoordinator : ILogSubject
{
    /// <summary>
    /// Logger instance for this coordinator
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// Service provider for resolving handler instances
    /// </summary>
    private readonly IServiceProvider _sp;

    /// <summary>
    /// Message sender for pushing messages to connections
    /// </summary>
    private readonly IMessageSender _sender;

    /// <summary>
    /// Collection of push routes mapping actions to their handlers
    /// </summary>
    private readonly IReadOnlyCollection<KeyValuePair<ActionKey, PushRoute>> _routes;

    public PushCoordinator(IServiceProvider sp, IMessageSender sender, RouteStore routeStore, ILogger logger)
    {
        _sp = sp;
        _sender = sender;
        Logger = logger;
        _routes = routeStore.PushRoutes.List();
    }

    /// <summary>
    /// Runs all push handlers for the specified connection
    /// </summary>
    /// <param name="cid">Connection identifier</param>
    /// <param name="cn">The sending connection</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>A task that represents the completion of all push handlers</returns>
    public Task RunAsync(Guid cid, ISendingConnection cn, CancellationToken ct) =>
        Task.WhenAll(_routes.Select(x => RunRouteAsync(x.Key, x.Value, cid, cn, ct)));

    /// <summary>
    /// Runs a specific push handler for the given route and connection
    /// </summary>
    /// <param name="actionKey">The action key for the push handler</param>
    /// <param name="route">The push route configuration</param>
    /// <param name="cid">Connection identifier</param>
    /// <param name="cn">The sending connection</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>A task that represents the completion of the push handler</returns>
    private async Task RunRouteAsync(
        ActionKey actionKey,
        PushRoute route,
        Guid cid,
        ISendingConnection cn,
        CancellationToken ct
    )
    {
        this.Trace("start");

        this.Trace<string>("resolve handler {handlerType}", route.HandlerType.FriendlyName());
        var handler = _sp.Resolve(route.HandlerType);

        var contextType = typeof(PushContext<>).MakeGenericType(route.MessageType);
        this.Trace<string>("create context {contextType}", contextType.FriendlyName());
        var context = Activator.CreateInstance(contextType, _sender, actionKey, cid, cn, ct, Logger);

        // execute and resolve result data
        this.Trace<string>("execute handler {handlerType}", route.HandlerType.FriendlyName());
        var resultTask = route.HandleMethod.Invoke(handler, new[] { context, ct })!;
        await (Task)resultTask;

        this.Trace("done");
    }
}
