using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Core.Mediator;
using Annium.Data.Operations;
using Annium.Extensions.Execution;
using Annium.Infrastructure.WebSockets.Domain.Models;
using Annium.Infrastructure.WebSockets.Domain.Requests;
using Annium.Infrastructure.WebSockets.Domain.Responses;
using Annium.Infrastructure.WebSockets.Server.Internal.Responses;
using Annium.Infrastructure.WebSockets.Server.Models;

namespace Annium.Infrastructure.WebSockets.Server.Internal.Models
{
    internal sealed record SubscriptionContext<TInit, TMessage, TState> :
        ISubscriptionContext<TInit, TMessage, TState>,
        IAsyncDisposable
        where TInit : SubscriptionInitRequestBase
        where TState : ConnectionStateBase
    {
        public TInit Request { get; }
        public TState State => _state;
        public Guid ConnectionId { get; }
        public Guid SubscriptionId { get; }
        private readonly CancellationTokenSource _cts;
        private readonly IMediator _mediator;
        private readonly TState _state;
        private bool _isInitiated;
        private Action _handleInit = () => { };
        private readonly IBackgroundExecutor _executor = Executor.Background.Sequential();

        public SubscriptionContext(
            TInit request,
            TState state,
            Guid subscriptionId,
            CancellationTokenSource cts,
            IMediator mediator
        )
        {
            Request = request;
            ConnectionId = state.ConnectionId;
            SubscriptionId = subscriptionId;
            _cts = cts;
            _mediator = mediator;
            _state = state;
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
                throw new InvalidOperationException("Can't send message from canceled subscription");

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

        public async ValueTask DisposeAsync()
        {
            if (!_isInitiated)
                throw new InvalidOperationException("Can't cancel not initiated subscription");
            if (_cts.IsCancellationRequested)
                throw new InvalidOperationException("Can't cancel subscription more than once");

            _cts.Cancel();
            _cts.Dispose();
            await _executor.DisposeAsync();
        }

        private void SendInternal<T>(T msg) =>
            _executor.Schedule(() => _mediator.SendAsync<Unit>(PushMessage.New(_state.ConnectionId, msg), CancellationToken.None));
    }
}