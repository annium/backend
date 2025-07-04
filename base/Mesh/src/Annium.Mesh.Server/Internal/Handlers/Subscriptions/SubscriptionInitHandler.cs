// using System;
// using System.Threading;
// using System.Threading.Tasks;
// using Annium.Core.Mediator;
// using Annium.Logging;
// using Annium.Mesh.Domain.Requests;
// using Annium.Mesh.Server.Internal.Models;
// using Annium.Mesh.Server.Internal.Responses;
// //
// namespace Annium.Mesh.Server.Internal.Handlers.Subscriptions;
//
// internal class SubscriptionInitHandler<TInit, TMessage> :
//     IPipeRequestHandler<
//         RequestContext<TInit>,
//         ISubscriptionContext<TInit, TMessage>,
//         None,
//         VoidResponse<TMessage>>,
//     ILogSubject
//     where TInit : SubscriptionInitRequestBaseObsolete
// {
//     public ILogger Logger { get; }
//     private readonly SubscriptionContextStore _subscriptionContextStore;
//     private readonly IMediator _mediator;
//     private readonly IServiceProvider _sp;
//
//     public SubscriptionInitHandler(
//         SubscriptionContextStore subscriptionContextStore,
//         IMediator mediator,
//         ILogger logger,
//         IServiceProvider sp
//     )
//     {
//         _subscriptionContextStore = subscriptionContextStore;
//         _mediator = mediator;
//         Logger = logger;
//         _sp = sp;
//     }
//
//     public async Task<VoidResponse<TMessage>> HandleAsync(
//         RequestContext<TInit> ctx,
//         CancellationToken ct,
//         Func<ISubscriptionContext<TInit, TMessage>, CancellationToken, Task<None>> next
//     )
//     {
//         var subscriptionId = ctx.Request.Rid;
//         this.Trace("subscription {subId} - init", subscriptionId);
//         var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
//         await using var context = new SubscriptionContext<TInit, TMessage>(
//             ctx.ConnectionId,
//             subscriptionId,
//             ctx.Request,
//             cts,
//             _mediator,
//             Logger,
//             _sp
//         );
//
//         // when reporting successful init - save to subscription store
//         context.OnInit(() =>
//         {
//             this.Trace("subscription {subId} - save to store", subscriptionId);
//             // ReSharper disable once AccessToDisposedClosure
//             _subscriptionContextStore.Save(context);
//         });
//
//         // run subscription
//         try
//         {
//             this.Trace("subscription {subId} - start next", subscriptionId);
//             await next(context, cts.Token);
//             this.Trace("subscription {subId} - done next", subscriptionId);
//         }
//         catch (Exception e)
//         {
//             this.Error(e);
//         }
//
//         return Response.Void<TMessage>();
//     }
// }
