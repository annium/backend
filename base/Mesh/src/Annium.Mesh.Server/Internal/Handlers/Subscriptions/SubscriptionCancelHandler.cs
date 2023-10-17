using System.Threading;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Core.Mediator;
using Annium.Data.Operations;
using Annium.Logging;
using Annium.Mesh.Domain.Requests;
using Annium.Mesh.Domain.Responses;
using Annium.Mesh.Server.Internal.Responses;
using Annium.Mesh.Server.Models;

namespace Annium.Mesh.Server.Internal.Handlers.Subscriptions;

internal class SubscriptionCancelHandler :
    IFinalRequestHandler<
        IRequestContext<SubscriptionCancelRequestObsolete>,
        ResultResponseObsolete
    >,
    ILogSubject
{
    public ILogger Logger { get; }
    private readonly SubscriptionContextStore _subscriptionContextStore;

    public SubscriptionCancelHandler(
        SubscriptionContextStore subscriptionContextStore,
        ILogger logger
    )
    {
        _subscriptionContextStore = subscriptionContextStore;
        Logger = logger;
    }

    public Task<ResultResponseObsolete> HandleAsync(
        IRequestContext<SubscriptionCancelRequestObsolete> ctx,
        CancellationToken ct
    )
    {
        var subscriptionId = ctx.Request.SubscriptionId;
        this.Trace("subscription {subId} - init", subscriptionId);
        var status = _subscriptionContextStore.TryCancel(subscriptionId)
            ? OperationStatus.Ok
            : OperationStatus.NotFound;
        this.Trace("subscription {subId} - result: {status}", subscriptionId, status);
        var response = Response.Result(ctx.Request.Rid, Result.Status(status));

        return Task.FromResult(response);
    }
}