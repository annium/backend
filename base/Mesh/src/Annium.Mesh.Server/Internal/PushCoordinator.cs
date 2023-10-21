using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Logging;
using Annium.Mesh.Serialization.Abstractions;
using Annium.Mesh.Server.Internal.Models;
using Annium.Mesh.Server.Internal.Routing;
using Annium.Mesh.Transport.Abstractions;

namespace Annium.Mesh.Server.Internal;

internal class PushCoordinator : ILogSubject
{
    private readonly IServiceProvider _sp;
    private readonly ISerializer _serializer;
    public ILogger Logger { get; }
    private readonly IReadOnlyCollection<KeyValuePair<ActionKey, PushRoute>> _routes;

    public PushCoordinator(
        IServiceProvider sp,
        ISerializer serializer,
        RouteStore routeStore,
        ILogger logger
    )
    {
        _sp = sp;
        _serializer = serializer;
        Logger = logger;
        _routes = routeStore.PushRoutes.List();
    }

    public Task RunAsync(Guid cid, ISendingConnection cn, CancellationToken ct) =>
        Task.WhenAll(_routes.Select(x => RunRouteAsync(x.Key, x.Value, cid, cn, ct)));

    private async Task RunRouteAsync(ActionKey actionKey, PushRoute route, Guid cid, ISendingConnection cn, CancellationToken ct)
    {
        this.Trace("start");

        this.Trace<string>("resolve handler {handlerType}", route.HandlerType.FriendlyName());
        var handler = _sp.Resolve(route.HandlerType);

        var contextType = typeof(PushContext<>).MakeGenericType(route.MessageType);
        this.Trace<string>("create context {contextType}", contextType.FriendlyName());
        var context = Activator.CreateInstance(contextType, actionKey, _serializer, cid, cn, ct, Logger);

        // execute and resolve result data
        this.Trace<string>("execute handler {handlerType}", route.HandlerType.FriendlyName());
        var resultTask = route.HandleMethod.Invoke(handler, new[] { context, ct })!;
        await (Task)resultTask;

        this.Trace("done");
    }
}