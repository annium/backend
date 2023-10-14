using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Core.Mediator;
using Annium.Data.Operations;
using Annium.Extensions.Execution;
using Annium.Logging;
using Annium.Mesh.Domain.Requests;
using Annium.Mesh.Domain.Responses;
using Annium.Mesh.Server.Internal.Responses;
using Annium.Mesh.Server.Models;

namespace Annium.Mesh.Server.Internal.Models;

internal sealed record SubscriptionContext<TInit, TMessage, TState> :
    ISubscriptionContext<TInit, TMessage, TState>,
    ISubscriptionContext,
    ILogSubject
    where TInit : SubscriptionInitRequestBase
    where TState : ConnectionStateBase
{
    public TInit Request { get; }
    public TState State { get; }
    public Guid ConnectionId { get; }
    public Guid SubscriptionId { get; }
    public ILogger Logger { get; }
    private readonly CancellationTokenSource _cts;
    private readonly IMediator _mediator;
    private readonly IServiceProvider _sp;
    private bool _isInitiated;
    private Action _handleInit = () => { };
    private readonly IBackgroundExecutor _executor;

    public SubscriptionContext(
        TInit request,
        TState state,
        Guid subscriptionId,
        CancellationTokenSource cts,
        IMediator mediator,
        ILogger logger,
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
        _executor = Executor.Background.Sequential<SubscriptionContext<TInit, TMessage, TState>>(logger);
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
        {
            this.Trace("connection {connectionId}, subscription {subscriptionId} - skip sending, cancellation requested");
            return;
        }

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

        this.Trace("connection {connectionId}, subscription {subscriptionId} - start", ConnectionId, SubscriptionId);
        _cts.Dispose();
        this.Trace("connection {connectionId}, subscription {subscriptionId} - dispose executor", ConnectionId, SubscriptionId);
        await _executor.DisposeAsync();
        this.Trace("connection {connectionId}, subscription {subscriptionId} - done", ConnectionId, SubscriptionId);
    }

    private void SendInternal<T>(T msg)
    {
        this.Trace("connection {connectionId}, subscription {subscriptionId} - schedule message {message}", ConnectionId, SubscriptionId, msg);
        _executor.Schedule(() =>
        {
            this.Trace("connection {connectionId}, subscription {subscriptionId} - send message {message}", ConnectionId, SubscriptionId, msg);
            _mediator.SendAsync<None>(_sp, PushMessage.New(State.ConnectionId, msg), CancellationToken.None);
        });
    }
}

internal interface ISubscriptionContext : IAsyncDisposable
{
    Guid ConnectionId { get; }
    Guid SubscriptionId { get; }
    void Cancel();
}