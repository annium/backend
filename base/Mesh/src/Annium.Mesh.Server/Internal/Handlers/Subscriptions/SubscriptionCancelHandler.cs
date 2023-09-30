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

internal class SubscriptionCancelHandler<TState> :
    IFinalRequestHandler<
        IRequestContext<SubscriptionCancelRequest, TState>,
        ResultResponse
    >,
    ILogSubject
    where TState : ConnectionStateBase
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

    public Task<ResultResponse> HandleAsync(
        IRequestContext<SubscriptionCancelRequest, TState> ctx,
        CancellationToken ct
    )
    {
        var subscriptionId = ctx.Request.SubscriptionId;
        this.Trace("subscription {subscriptionId} - init", subscriptionId);
        var status = _subscriptionContextStore.TryCancel(subscriptionId)
            ? OperationStatus.Ok
            : OperationStatus.NotFound;
        this.Trace("subscription {subscriptionId} - result: {status}", subscriptionId, status);
        var response = Response.Result(ctx.Request.Rid, Result.Status(status));

        return Task.FromResult(response);
    }
}