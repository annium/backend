// using System.Threading;
// using System.Threading.Tasks;
// using Annium.Architecture.Base;
// using Annium.Data.Operations;
// using Annium.Logging;
// // // using Annium.Mesh.Tests.System.Domain;
// using Annium.Threading;
//
// namespace Annium.Mesh.Tests.System.Server.Demo;
//
// internal class SecondSubscriptionHandler :
//     ISubscriptionHandler<SecondSubscriptionInit, string>,
//     ILogSubject
// {
//     public ILogger Logger { get; }
//     private readonly SharedDataContainer _container;
//
//     public SecondSubscriptionHandler(
//         SharedDataContainer container,
//         ILogger logger
//     )
//     {
//         Logger = logger;
//         _container = container;
//     }
//
//     public async Task<None> HandleAsync(
//         ISubscriptionContext<SecondSubscriptionInit, string> ctx,
//         CancellationToken ct
//     )
//     {
//         this.Trace("start");
//         _container.Log.Enqueue($"second init: {ctx.Request.Param}");
//         ctx.Handle(Result.Status(OperationStatus.Ok));
//
//         this.Trace("msg1");
//         _container.Log.Enqueue("second msg1");
//         ctx.Send("second msg1");
//
//         this.Trace("msg2");
//         _container.Log.Enqueue("second msg2");
//         ctx.Send("second msg2");
//
//         this.Trace("await cancellation");
//         await ct;
//
//         this.Trace("report canceled");
//         _container.Log.Enqueue("second canceled");
//
//         return None.Default;
//     }
// }
