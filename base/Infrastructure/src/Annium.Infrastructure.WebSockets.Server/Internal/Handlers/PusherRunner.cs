using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Mediator;
using Annium.Infrastructure.WebSockets.Domain.Responses;
using Annium.Infrastructure.WebSockets.Server.Handlers;
using Annium.Infrastructure.WebSockets.Server.Internal.Models;
using Annium.Infrastructure.WebSockets.Server.Models;
using Annium.Logging.Abstractions;

namespace Annium.Infrastructure.WebSockets.Server.Internal.Handlers;

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