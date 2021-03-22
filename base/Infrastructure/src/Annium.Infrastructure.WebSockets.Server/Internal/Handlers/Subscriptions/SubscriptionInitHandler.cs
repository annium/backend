using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Mediator;
using Annium.Infrastructure.WebSockets.Domain.Models;
using Annium.Infrastructure.WebSockets.Domain.Requests;
using Annium.Infrastructure.WebSockets.Server.Internal.Models;
using Annium.Infrastructure.WebSockets.Server.Internal.Responses;
using Annium.Infrastructure.WebSockets.Server.Models;

namespace Annium.Infrastructure.WebSockets.Server.Internal.Handlers.Subscriptions
{
    internal class SubscriptionInitHandler<TInit, TMessage> :
        IPipeRequestHandler<
            RequestContext<TInit>,
            ISubscriptionContext<TInit, TMessage>,
            Unit,
            VoidResponse<TMessage>
        >
        where TInit : SubscriptionInitRequestBase
    {
        private readonly SubscriptionContextStore<TInit, TMessage> _subscriptionContextStore;
        private readonly IMediator _mediator;

        public SubscriptionInitHandler(
            SubscriptionContextStore<TInit, TMessage> subscriptionContextStore,
            IMediator mediator
        )
        {
            _subscriptionContextStore = subscriptionContextStore;
            _mediator = mediator;
        }

        public async Task<VoidResponse<TMessage>> HandleAsync(
            RequestContext<TInit> ctx,
            CancellationToken ct,
            Func<ISubscriptionContext<TInit, TMessage>, Task<Unit>> next
        )
        {
            var subscriptionId = Guid.NewGuid();
            var context = new SubscriptionContext<TInit, TMessage>(ctx.Request, ctx.StateInternal, subscriptionId, _mediator);

            // when reporting successful init - save to subscription store
            context.OnInit(() => _subscriptionContextStore.Save(context));

            // run subscription
            await next(context);

            // remove subscription to prevent memory leak (if finished on server, but not canceled by client)
            _subscriptionContextStore.TryRemove(subscriptionId);

            return Response.Void<TMessage>();
        }
    }
}