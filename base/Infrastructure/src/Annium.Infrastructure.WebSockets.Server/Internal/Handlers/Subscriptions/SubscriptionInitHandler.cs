using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Internal;
using Annium.Core.Mediator;
using Annium.Infrastructure.WebSockets.Domain.Models;
using Annium.Infrastructure.WebSockets.Domain.Requests;
using Annium.Infrastructure.WebSockets.Server.Internal.Models;
using Annium.Infrastructure.WebSockets.Server.Internal.Responses;
using Annium.Infrastructure.WebSockets.Server.Models;

namespace Annium.Infrastructure.WebSockets.Server.Internal.Handlers.Subscriptions
{
    internal class SubscriptionInitHandler<TInit, TMessage, TState> :
        IPipeRequestHandler<
            RequestContext<TInit, TState>,
            ISubscriptionContext<TInit, TMessage, TState>,
            Unit,
            VoidResponse<TMessage>
        >
        where TInit : SubscriptionInitRequestBase
        where TState : ConnectionStateBase
    {
        private readonly SubscriptionContextStore _subscriptionContextStore;
        private readonly IMediator _mediator;

        public SubscriptionInitHandler(
            SubscriptionContextStore subscriptionContextStore,
            IMediator mediator
        )
        {
            _subscriptionContextStore = subscriptionContextStore;
            _mediator = mediator;
        }

        public async Task<VoidResponse<TMessage>> HandleAsync(
            RequestContext<TInit, TState> ctx,
            CancellationToken ct,
            Func<ISubscriptionContext<TInit, TMessage, TState>, CancellationToken, Task<Unit>> next
        )
        {
            var subscriptionId = ctx.Request.SubscriptionId;
            this.Trace(() => $"subscription {subscriptionId} - init");
            var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            var context = new SubscriptionContext<TInit, TMessage, TState>(ctx.Request, ctx.State, subscriptionId, cts, _mediator);

            // when reporting successful init - save to subscription store
            context.OnInit(() =>
            {
                this.Trace(() => $"subscription {subscriptionId} - save to store");
                _subscriptionContextStore.Save(context);
            });

            // run subscription
            await next(context, cts.Token);

            // remove subscription to prevent memory leak (if finished on server, but not canceled by client)
            this.Trace(() => $"subscription {subscriptionId} - try remove");
            var result = await _subscriptionContextStore.TryRemove(subscriptionId);
            this.Trace(() => $"subscription {subscriptionId} - removed: {result}");

            return Response.Void<TMessage>();
        }
    }
}