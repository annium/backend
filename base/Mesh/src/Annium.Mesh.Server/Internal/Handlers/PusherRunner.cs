using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Mediator;
using Annium.Logging;
using Annium.Mesh.Domain.Responses;
using Annium.Mesh.Server.Handlers;
using Annium.Mesh.Server.Internal.Models;

namespace Annium.Mesh.Server.Internal.Handlers;

internal class PusherRunner<TMessage> : IPusherRunner
    where TMessage : NotificationBase
{
    private readonly IPusher<TMessage> _pusher;
    private readonly IMediator _mediator;
    private readonly ILogger _logger;
    private readonly IServiceProvider _sp;

    public PusherRunner(
        IPusher<TMessage> pusher,
        IMediator mediator,
        ILogger logger,
        IServiceProvider sp
    )
    {
        _pusher = pusher;
        _mediator = mediator;
        _logger = logger;
        _sp = sp;
    }

    public Task RunAsync(Guid cid, CancellationToken ct)
    {
        var ctx = new PushContext<TMessage>(cid, ct, _mediator, _logger, _sp);

        return _pusher.RunAsync(ctx, ct);
    }
}

internal interface IPusherRunner
{
    Task RunAsync(Guid cid, CancellationToken ct);
}