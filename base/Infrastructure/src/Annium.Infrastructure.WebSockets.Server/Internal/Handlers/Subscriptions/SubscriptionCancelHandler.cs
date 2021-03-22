using System.Threading;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Core.Mediator;
using Annium.Data.Operations;
using Annium.Infrastructure.WebSockets.Domain.Models;
using Annium.Infrastructure.WebSockets.Domain.Requests;
using Annium.Infrastructure.WebSockets.Domain.Responses;
using Annium.Infrastructure.WebSockets.Server.Internal.Responses;

namespace Annium.Infrastructure.WebSockets.Server.Internal.Handlers.Subscriptions
{
    internal class SubscriptionCancelHandler<TInit, TMessage> :
        IFinalRequestHandler<
            IRequestContext<SubscriptionCancelRequest>,
            MetaResponse<TInit, TMessage, ResultResponse>
        >
        where TInit : SubscriptionInitRequestBase
    {
        private readonly SubscriptionContextStore<TInit, TMessage> _subscriptionContextStore;

        public SubscriptionCancelHandler(
            SubscriptionContextStore<TInit, TMessage> subscriptionContextStore
        )
        {
            _subscriptionContextStore = subscriptionContextStore;
        }

        public Task<MetaResponse<TInit, TMessage, ResultResponse>> HandleAsync(
            IRequestContext<SubscriptionCancelRequest> ctx,
            CancellationToken ct
        )
        {
            var status = _subscriptionContextStore.TryRemove(ctx.Request.SubscriptionId)
                ? OperationStatus.Ok
                : OperationStatus.NotFound;
            var response = Response.Result(ctx.Request.Rid, Result.Status(status));

            return Task.FromResult(Response.Meta<TInit, TMessage, ResultResponse>(response));
        }
    }
}