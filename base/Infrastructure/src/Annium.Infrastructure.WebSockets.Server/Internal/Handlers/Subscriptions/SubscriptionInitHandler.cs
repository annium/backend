using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Mediator;
using Annium.Infrastructure.WebSockets.Domain.Requests;
using Annium.Infrastructure.WebSockets.Server.Internal.Models;
using Annium.Infrastructure.WebSockets.Server.Internal.Responses;
using Annium.Infrastructure.WebSockets.Server.Models;
using Annium.Logging;

namespace Annium.Infrastructure.WebSockets.Server.Internal.Handlers.Subscriptions;

internal class SubscriptionInitHandler<TInit, TMessage, TState> :
    IPipeRequestHandler<
        RequestContext<TInit, TState>,
        ISubscriptionContext<TInit, TMessage, TState>,
        None,
        VoidResponse<TMessage>
    >,
    ILogSubject
    where TInit : SubscriptionInitRequestBase
    where TState : ConnectionStateBase
{
    public ILogger Logger { get; }
    private readonly SubscriptionContextStore _subscriptionContextStore;
    private readonly IMediator _mediator;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IServiceProvider _sp;

    public SubscriptionInitHandler(
        SubscriptionContextStore subscriptionContextStore,
        IMediator mediator,
        ILogger logger,
        ILoggerFactory loggerFactory,
        IServiceProvider sp
    )
    {
        _subscriptionContextStore = subscriptionContextStore;
        _mediator = mediator;
        Logger = logger;
        _loggerFactory = loggerFactory;
        _sp = sp;
    }

    public async Task<VoidResponse<TMessage>> HandleAsync(
        RequestContext<TInit, TState> ctx,
        CancellationToken ct,
        Func<ISubscriptionContext<TInit, TMessage, TState>, CancellationToken, Task<None>> next
    )
    {
        var subscriptionId = ctx.Request.Rid;
        this.Trace($"subscription {subscriptionId} - init");
        var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        await using var context = new SubscriptionContext<TInit, TMessage, TState>(
            ctx.Request,
            ctx.State,
            subscriptionId,
            cts,
            _mediator,
            _loggerFactory.Get<SubscriptionContext<TInit, TMessage, TState>>(),
            _sp
        );

        // when reporting successful init - save to subscription store
        context.OnInit(() =>
        {
            this.Trace($"subscription {subscriptionId} - save to store");
            // ReSharper disable once AccessToDisposedClosure
            _subscriptionContextStore.Save(context);
        });

        // run subscription
        try
        {
            await next(context, cts.Token);
        }
        catch (Exception e)
        {
            this.Error(e);
        }

        return Response.Void<TMessage>();
    }
}