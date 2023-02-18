using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Core.Mediator;
using Annium.Data.Operations;
using Annium.Extensions.Execution;
using Annium.Infrastructure.WebSockets.Domain.Requests;
using Annium.Infrastructure.WebSockets.Domain.Responses;
using Annium.Infrastructure.WebSockets.Server.Internal.Responses;
using Annium.Infrastructure.WebSockets.Server.Models;
using Annium.Logging.Abstractions;

namespace Annium.Infrastructure.WebSockets.Server.Internal.Models;

internal sealed record SubscriptionContext<TInit, TMessage, TState> :
    ISubscriptionContext<TInit, TMessage, TState>,
    ISubscriptionContext,
    ILogSubject<SubscriptionContext<TInit, TMessage, TState>>
    where TInit : SubscriptionInitRequestBase
    where TState : ConnectionStateBase
{
    public TInit Request { get; }
    public TState State { get; }
    public Guid ConnectionId { get; }
    public Guid SubscriptionId { get; }
    public ILogger<SubscriptionContext<TInit, TMessage, TState>> Logger { get; }
    private readonly CancellationTokenSource _cts;
    private readonly IMediator _mediator;
    private readonly IServiceProvider _sp;
    private bool _isInitiated;
    private Action _handleInit = () => { };
    private readonly IBackgroundExecutor _executor = Executor.Background.Sequential<SubscriptionContext<TInit, TMessage, TState>>();

    public SubscriptionContext(
        TInit request,
        TState state,
        Guid subscriptionId,
        CancellationTokenSource cts,
        IMediator mediator,
        ILogger<SubscriptionContext<TInit, TMessage, TState>> logger,
        IServiceProvider sp
    )
    {
        Request = request;
        State = state;
        ConnectionId = state.ConnectionId;
        SubscriptionId = subscriptionId;
        _cts = cts;
        _mediator = mediator;
        Logger = logger;
        _sp = sp;
        _executor.Start();
    }

    public void Handle(IStatusResult<OperationStatus> result)
    {
        if (_isInitiated)
            throw new InvalidOperationException("Can't init subscription more than once");
        _isInitiated = true;
        if (_cts.IsCancellationRequested)
            throw new InvalidOperationException("Can't init canceled subscription");

        if (result.IsOk)
            _handleInit();

        var responseResult = result.IsOk
            ? Result.Status(result.Status, SubscriptionId)
            : Result.Status(result.Status, Guid.Empty);
        SendInternal(Response.Result(Request.Rid, responseResult.Join(result)));
    }

    public void Send(TMessage message)
    {
        if (!_isInitiated)
            throw new InvalidOperationException("Can't send message from not initiated subscription");
        if (_cts.IsCancellationRequested)
            return;

        SendInternal(new SubscriptionMessage<TMessage>(SubscriptionId, message));
    }

    public void OnInit(Action handle)
    {
        _handleInit = handle;
    }

    public void Deconstruct(
        out TInit request,
        out TState state
    )
    {
        request = Request;
        state = State;
    }

    public void Cancel()
    {
        if (_cts.IsCancellationRequested)
            throw new InvalidOperationException("Can't cancel subscription more than once");
        _cts.Cancel();
    }

    public async ValueTask DisposeAsync()
    {
        if (!_isInitiated)
            throw new InvalidOperationException("Can't cancel not initiated subscription");

        this.Log().Trace($"connection {ConnectionId}, subscription {SubscriptionId} - start");
        _cts.Dispose();
        this.Log().Trace($"connection {ConnectionId}, subscription {SubscriptionId} - dispose executor");
        await _executor.DisposeAsync();
        this.Log().Trace($"connection {ConnectionId}, subscription {SubscriptionId} - done");
    }

    private void SendInternal<T>(T msg) =>
        _executor.Schedule(() => _mediator.SendAsync<None>(_sp, PushMessage.New(State.ConnectionId, msg), CancellationToken.None));
}

internal interface ISubscriptionContext : IAsyncDisposable
{
    Guid ConnectionId { get; }
    Guid SubscriptionId { get; }
    void Cancel();
}