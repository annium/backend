using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Mediator;
using Annium.Extensions.Execution;
using Annium.Logging;
using Annium.Mesh.Domain.Responses;
using Annium.Mesh.Server.Models;

namespace Annium.Mesh.Server.Internal.Models;

internal class PushContext<TMessage, TState> : IPushContext<TMessage, TState>, ILogSubject
    where TMessage : NotificationBase
    where TState : ConnectionStateBase
{
    private readonly CancellationToken _ct;
    private readonly IMediator _mediator;
    private readonly IServiceProvider _sp;
    public TState State { get; }
    public ILogger Logger { get; }
    private readonly IBackgroundExecutor _executor;

    public PushContext(
        TState state,
        CancellationToken ct,
        IMediator mediator,
        ILogger logger,
        IServiceProvider sp
    )
    {
        _ct = ct;
        _mediator = mediator;
        _sp = sp;
        State = state;
        Logger = logger;
        _executor = Executor.Background.Sequential<PushContext<TMessage, TState>>(logger);
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
        this.Trace("connection {connectionId} - start", State.ConnectionId);
        await _executor.DisposeAsync();
        this.Trace("connection {connectionId} - done", State.ConnectionId);
    }

    private void SendInternal<T>(T msg) =>
        _executor.Schedule(() => _mediator.SendAsync<None>(_sp, PushMessage.New(State.ConnectionId, msg), CancellationToken.None));
}