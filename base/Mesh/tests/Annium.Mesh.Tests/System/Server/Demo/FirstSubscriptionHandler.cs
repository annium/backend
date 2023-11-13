// using System.Threading;
// using System.Threading.Tasks;
// using Annium.Architecture.Base;
// using Annium.Data.Operations;
// using Annium.Logging;
// using Annium.Mesh.Server.Handlers;
// using Annium.Mesh.Server.Models;
// using Annium.Mesh.Tests.System.Domain;
// using Annium.Threading;
//
// namespace Annium.Mesh.Tests.System.Server.Demo;
//
// internal class FirstSubscriptionHandler :
//     ISubscriptionHandler<FirstSubscriptionInit, string>,
//     ILogSubject
// {
//     public ILogger Logger { get; }
//     private readonly SharedDataContainer _container;
//
//     public FirstSubscriptionHandler(
//         SharedDataContainer container,
//         ILogger logger
//     )
//     {
//         Logger = logger;
//         _container = container;
//     }
//
//     public async Task<None> HandleAsync(
//         ISubscriptionContext<FirstSubscriptionInit, string> ctx,
//         CancellationToken ct
//     )
//     {
//         this.Trace("start");
//         _container.Log.Enqueue($"first init: {ctx.Request.Param}");
//         ctx.Handle(Result.Status(OperationStatus.Ok));
//
//         this.Trace("msg1");
//         _container.Log.Enqueue("first msg1");
//         ctx.Send("first msg1");
//
//         this.Trace("msg2");
//         _container.Log.Enqueue("first msg2");
//         ctx.Send("first msg2");
//
//         this.Trace("await cancellation");
//         await ct;
//
//         this.Trace("report canceled");
//         _container.Log.Enqueue("first canceled");
//
//         return None.Default;
//     }
// }
