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
        private readonly IServiceProvider _sp;

        public SubscriptionInitHandler(
            SubscriptionContextStore subscriptionContextStore,
            IMediator mediator,
            IServiceProvider sp
        )
        {
            _subscriptionContextStore = subscriptionContextStore;
            _mediator = mediator;
            _sp = sp;
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
            await using var context = new SubscriptionContext<TInit, TMessage, TState>(ctx.Request, ctx.State, subscriptionId, cts, _mediator, _sp);

            // when reporting successful init - save to subscription store
            context.OnInit(() =>
            {
                this.Trace(() => $"subscription {subscriptionId} - save to store");
                _subscriptionContextStore.Save(context);
            });

            // run subscription
            await next(context, cts.Token);

            return Response.Void<TMessage>();
        }
    }
}