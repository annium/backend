using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Mediator;
using Annium.Extensions.Execution;
using Annium.Logging;
using Annium.Mesh.Domain.Responses;
using Annium.Mesh.Server.Models;

namespace Annium.Mesh.Server.Internal.Models;

internal class PushContext<TMessage> : IPushContext<TMessage>, ILogSubject
    where TMessage : NotificationBase
{
    public ILogger Logger { get; }
    private readonly CancellationToken _ct;
    private readonly IMediator _mediator;
    private readonly IServiceProvider _sp;
    private readonly IBackgroundExecutor _executor;
    private readonly ConnectionState _state;

    public PushContext(
        ConnectionState state,
        CancellationToken ct,
        IMediator mediator,
        ILogger logger,
        IServiceProvider sp
    )
    {
        _ct = ct;
        _mediator = mediator;
        _sp = sp;
        _state = state;
        Logger = logger;
        _executor = Executor.Background.Sequential<PushContext<TMessage>>(logger);
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
        this.Trace("connection {connectionId} - start", _state.ConnectionId);
        await _executor.DisposeAsync();
        this.Trace("connection {connectionId} - done", _state.ConnectionId);
    }

    private void SendInternal<T>(T msg) =>
        _executor.Schedule(() => _mediator.SendAsync<None>(_sp, PushMessage.New(_state.ConnectionId, msg), CancellationToken.None));
}