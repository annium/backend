using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Mediator;
using Annium.Logging;
using Annium.Mesh.Domain.Responses;
using Annium.Mesh.Server.Handlers;
using Annium.Mesh.Server.Internal.Models;
using Annium.Mesh.Server.Models;

namespace Annium.Mesh.Server.Internal.Handlers;

internal class PusherRunner<TMessage, TState> : IPusherRunner<TState>
    where TMessage : NotificationBase
    where TState : ConnectionStateBase
{
    private readonly IPusher<TMessage, TState> _pusher;
    private readonly IMediator _mediator;
    private readonly ILogger _logger;
    private readonly IServiceProvider _sp;

    public PusherRunner(
        IPusher<TMessage, TState> pusher,
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

    public Task RunAsync(TState state, CancellationToken ct)
    {
        var ctx = new PushContext<TMessage, TState>(state, ct, _mediator, _logger, _sp);

        return _pusher.RunAsync(ctx, ct);
    }
}

internal interface IPusherRunner<TState>
    where TState : ConnectionStateBase
{
    Task RunAsync(TState state, CancellationToken ct);
}