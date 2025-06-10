using System;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.Mesh.Server.Internal.Handlers;

/// <summary>
/// Defines a contract for running pushers in the mesh server infrastructure.
/// </summary>
internal interface IPusherRunner
{
    /// <summary>
    /// Runs the pusher for the specified connection.
    /// </summary>
    /// <param name="cid">The connection identifier.</param>
    /// <param name="ct">The cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous push operation.</returns>
    Task RunAsync(Guid cid, CancellationToken ct);
}

// internal class PusherRunner<TMessage> : IPusherRunner
//     where TMessage : NotificationBaseObsolete
// {
//     private readonly IPusher<TMessage> _pusher;
//     private readonly IMediator _mediator;
//     private readonly ILogger _logger;
//     private readonly IServiceProvider _sp;
//
//     public PusherRunner(
//         IPusher<TMessage> pusher,
//         IMediator mediator,
//         ILogger logger,
//         IServiceProvider sp
//     )
//     {
//         _pusher = pusher;
//         _mediator = mediator;
//         _logger = logger;
//         _sp = sp;
//     }
//
//     public Task RunAsync(Guid cid, CancellationToken ct)
//     {
//         var ctx = new PushContext<TMessage>(cid, ct, _mediator, _logger, _sp);
//
//         return _pusher.RunAsync(ctx, ct);
//     }
// }
