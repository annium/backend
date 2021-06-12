using System.Threading;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Core.Mediator;
using Annium.Data.Operations;
using Annium.Infrastructure.WebSockets.Domain.Models;
using Annium.Infrastructure.WebSockets.Domain.Requests;
using Annium.Infrastructure.WebSockets.Domain.Responses;
using Annium.Infrastructure.WebSockets.Server.Internal.Responses;
using Annium.Logging.Abstractions;

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
        private readonly ILogger<SubscriptionCancelHandler<TState>> _logger;

        public SubscriptionCancelHandler(
            SubscriptionContextStore subscriptionContextStore,
            ILogger<SubscriptionCancelHandler<TState>> logger
        )
        {
            _subscriptionContextStore = subscriptionContextStore;
            _logger = logger;
        }

        public Task<ResultResponse> HandleAsync(
            IRequestContext<SubscriptionCancelRequest, TState> ctx,
            CancellationToken ct
        )
        {
            var subscriptionId = ctx.Request.SubscriptionId;
            _logger.Trace($"subscription {subscriptionId} - init");
            var status = _subscriptionContextStore.TryCancel(subscriptionId)
                ? OperationStatus.Ok
                : OperationStatus.NotFound;
            _logger.Trace($"subscription {subscriptionId} - result: {status}");
            var response = Response.Result(ctx.Request.Rid, Result.Status(status));

            return Task.FromResult(response);
        }
    }
}