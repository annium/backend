using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Core.Mediator;
using Annium.Data.Operations;
using Annium.Infrastructure.WebSockets.Domain.Models;
using Annium.Infrastructure.WebSockets.Domain.Requests;
using Annium.Infrastructure.WebSockets.Domain.Responses;
using Annium.Infrastructure.WebSockets.Server.Internal.Responses;
using Annium.Infrastructure.WebSockets.Server.Models;

namespace Annium.Infrastructure.WebSockets.Server.Internal.Models
{
    internal sealed record SubscriptionContext<TInit, TMessage> :
        ISubscriptionContext<TInit, TMessage>,
        IDisposable
        where TInit : SubscriptionInitRequestBase
    {
        public TInit Request { get; }
        public IConnectionState State => _state;
        public Guid ConnectionId { get; }
        public Guid SubscriptionId { get; }
        public CancellationToken Token => _cts.Token;
        private readonly CancellationTokenSource _cts;
        private readonly ConnectionState _state;
        private readonly IMediator _mediator;
        private bool _isInitiated;
        private Action _handleInit = () => { };

        public SubscriptionContext(
            TInit request,
            ConnectionState state,
            Guid subscriptionId,
            IMediator mediator
        )
        {
            Request = request;
            ConnectionId = state.ConnectionId;
            SubscriptionId = subscriptionId;
            _cts = new CancellationTokenSource();
            _state = state;
            _mediator = mediator;
        }

        public async Task Handle(IStatusResult<OperationStatus> result)
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
            await _mediator.SendAsync<Unit>(
                PushMessage.New(_state.ConnectionId, Response.Result(Request.Rid, responseResult.Join(result))),
                CancellationToken.None
            );
        }

        public async Task Send(TMessage message)
        {
            if (!_isInitiated)
                throw new InvalidOperationException("Can't send message from not initiated subscription");
            if (_cts.IsCancellationRequested)
                throw new InvalidOperationException("Can't send message from canceled subscription");

            await _mediator.SendAsync<Unit>(
                PushMessage.New(_state.ConnectionId, new SubscriptionMessage<TMessage>(SubscriptionId, message)),
                CancellationToken.None
            );
        }

        public void Cancel()
        {
            if (!_isInitiated)
                throw new InvalidOperationException("Can't cancel not initiated subscription");
            if (_cts.IsCancellationRequested)
                throw new InvalidOperationException("Can't cancel subscription more than once");

            _cts.Cancel();
        }

        public void OnInit(Action handle)
        {
            _handleInit = handle;
        }

        public void Deconstruct(
            out TInit request,
            out IConnectionState state
        )
        {
            request = Request;
            state = State;
        }

        public void Dispose()
        {
            _cts.Dispose();
        }
    }
}