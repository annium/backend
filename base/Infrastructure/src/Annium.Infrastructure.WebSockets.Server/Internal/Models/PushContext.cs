using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Mediator;
using Annium.Core.Primitives;
using Annium.Extensions.Execution;
using Annium.Infrastructure.WebSockets.Domain.Responses;
using Annium.Infrastructure.WebSockets.Server.Models;
using Annium.Logging.Abstractions;

namespace Annium.Infrastructure.WebSockets.Server.Internal.Models;

internal class PushContext<TMessage, TState> : IPushContext<TMessage, TState>, ILogSubject<PushContext<TMessage, TState>>
    where TMessage : NotificationBase
    where TState : ConnectionStateBase
{
    private readonly CancellationToken _ct;
    private readonly IMediator _mediator;
    private readonly IServiceProvider _sp;
    public TState State { get; }
    public ILogger<PushContext<TMessage, TState>> Logger { get; }
    private readonly IBackgroundExecutor _executor = Executor.Background.Sequential<PushContext<TMessage, TState>>();

    public PushContext(
        TState state,
        CancellationToken ct,
        IMediator mediator,
        ILogger<PushContext<TMessage, TState>> logger,
        IServiceProvider sp
    )
    {
        _ct = ct;
        _mediator = mediator;
        _sp = sp;
        State = state;
        Logger = logger;
        _executor.Start();
    }


    public void Send(TMessage message)
    {
        if (_ct.IsCancellationRequested)
            return;

        SendInternal(message);
    }

    public async ValueTask DisposeAsync()
    {
        this.Log().Trace($"connection {State.ConnectionId} - start");
        await _executor.DisposeAsync();
        this.Log().Trace($"connection {State.ConnectionId} - done");
    }

    private void SendInternal<T>(T msg) =>
        _executor.Schedule(() => _mediator.SendAsync<None>(_sp, PushMessage.New(State.ConnectionId, msg), CancellationToken.None));
}