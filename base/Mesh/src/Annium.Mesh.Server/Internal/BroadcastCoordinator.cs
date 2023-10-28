// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading;
// using System.Threading.Tasks;
// using Annium.Logging;
// using Annium.Mesh.Serialization.Abstractions;
// using Annium.Mesh.Server.Internal.Handlers;
//
// namespace Annium.Mesh.Server.Internal;
//
// internal class BroadcastCoordinator : ILogSubject, IAsyncDisposable
// {
//     public ILogger Logger { get; }
//     private readonly ConnectionTracker _connectionTracker;
//     private readonly IEnumerable<IBroadcasterRunner> _runners;
//     private readonly ISerializer _serializer;
//     private readonly CancellationTokenSource _lifetimeCts;
//     private Task _runnersTask = Task.CompletedTask;
//
//     public BroadcastCoordinator(
//         ConnectionTracker connectionTracker,
//         IEnumerable<IBroadcasterRunner> runners,
//         ISerializer serializer,
//         ILogger logger
//     )
//     {
//         Logger = logger;
//         _connectionTracker = connectionTracker;
//         _runners = runners;
//         _serializer = serializer;
//         _lifetimeCts = new CancellationTokenSource();
//     }
//
//     public void Start()
//     {
//         Task.Run(async () =>
//         {
//             try
//             {
//                 await (_runnersTask = Task.WhenAll(_runners.Select(
//                     x => x.Run(Broadcast, _lifetimeCts.Token)
//                 )));
//             }
//             catch (Exception e)
//             {
//                 if (e is not TaskCanceledException)
//                     this.Error(e);
//             }
//         });
//     }
//
//     public async ValueTask DisposeAsync()
//     {
//         _lifetimeCts.Cancel();
//         try
//         {
//             await _runnersTask;
//         }
//         catch (Exception e)
//         {
//             if (e is not TaskCanceledException)
//                 this.Error(e);
//         }
//     }
//
//     private void Broadcast(object message)
//     {
//         var connections = _connectionTracker.GetSendingConnections();
//         if (connections.Count == 0)
//             return;
//
//         var data = _serializer.SerializeData(message);
//         Task.WhenAll(connections.Select(async x => await x.SendAsync(data)));
//     }
// }
