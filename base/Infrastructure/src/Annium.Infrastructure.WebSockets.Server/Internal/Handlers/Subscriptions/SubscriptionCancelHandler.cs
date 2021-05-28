using System.Threading;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Core.Internal;
using Annium.Core.Mediator;
using Annium.Data.Operations;
using Annium.Infrastructure.WebSockets.Domain.Models;
using Annium.Infrastructure.WebSockets.Domain.Requests;
using Annium.Infrastructure.WebSockets.Domain.Responses;
using Annium.Infrastructure.WebSockets.Server.Internal.Responses;

namespace Annium.Infrastructure.WebSockets.Server.Internal.Handlers.Subscriptions
{
    internal class SubscriptionCancelHandler<TState> :
        IFinalRequestHandler<
            IRequestContext<SubscriptionCancelRequest, TState>,
            ResultResponse
        >
        where TState : ConnectionStateBase
    {
        private readonly SubscriptionContextStore _subscriptionContextStore;

        public SubscriptionCancelHandler(
            SubscriptionContextStore subscriptionContextStore
        )
        {
            _subscriptionContextStore = subscriptionContextStore;
        }

        public async Task<ResultResponse> HandleAsync(
            IRequestContext<SubscriptionCancelRequest, TState> ctx,
            CancellationToken ct
        )
        {
            var subscriptionId = ctx.Request.SubscriptionId;
            this.Trace(() => $"subscription {subscriptionId} - init");
            var status = await _subscriptionContextStore.TryRemove(subscriptionId)
                ? OperationStatus.Ok
                : OperationStatus.NotFound;
            this.Trace(() => $"subscription {subscriptionId} - result: {status}");
            var response = Response.Result(ctx.Request.Rid, Result.Status(status));

            return response;
        }
    }
}