using System.Threading;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Core.Mediator;
using Annium.Data.Operations;
using Annium.Infrastructure.WebSockets.Domain.Requests;
using Annium.Infrastructure.WebSockets.Domain.Responses;
using Annium.Infrastructure.WebSockets.Server.Internal.Responses;
using Annium.Infrastructure.WebSockets.Server.Models;
using Annium.Logging.Abstractions;

namespace Annium.Infrastructure.WebSockets.Server.Internal.Handlers.Subscriptions;

internal class SubscriptionCancelHandler<TState> :
    IFinalRequestHandler<
        IRequestContext<SubscriptionCancelRequest, TState>,
        ResultResponse
    >,
    ILogSubject<SubscriptionCancelHandler<TState>>
    where TState : ConnectionStateBase
{
    public ILogger<SubscriptionCancelHandler<TState>> Logger { get; }
    private readonly SubscriptionContextStore _subscriptionContextStore;

    public SubscriptionCancelHandler(
        SubscriptionContextStore subscriptionContextStore,
        ILogger<SubscriptionCancelHandler<TState>> logger
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
        this.Log().Trace($"subscription {subscriptionId} - init");
        var status = _subscriptionContextStore.TryCancel(subscriptionId)
            ? OperationStatus.Ok
            : OperationStatus.NotFound;
        this.Log().Trace($"subscription {subscriptionId} - result: {status}");
        var response = Response.Result(ctx.Request.Rid, Result.Status(status));

        return Task.FromResult(response);
    }
}