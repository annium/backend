using System;
using System.Collections.Concurrent;
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
        IAsyncDisposable
        where TInit : SubscriptionInitRequestBase
    {
        public TInit Request { get; }
        public IConnectionState State => _state;
        public Guid ConnectionId { get; }
        public Guid SubscriptionId { get; }
        private readonly CancellationTokenSource _cts;
        private readonly ConnectionState _state;
        private readonly BlockingCollection<object> _events = new();
        private bool _isInitiated;
        private Action _handleInit = () => { };
        private Task _eventSenderTask;

        public SubscriptionContext(
            TInit request,
            ConnectionState state,
            Guid subscriptionId,
            CancellationTokenSource cts,
            IMediator mediator
        )
        {
            Request = request;
            ConnectionId = state.ConnectionId;
            SubscriptionId = subscriptionId;
            _cts = cts;
            _state = state;
            _eventSenderTask = Task.Run(async () =>
            {
                while (!_cts.IsCancellationRequested)
                {
                    try
                    {
                        var msg = _events.Take(_cts.Token);
                        await mediator.SendAsync<Unit>(msg, CancellationToken.None);
                    }
                    catch (OperationCanceledException)
                    {
                    }
                }
            }, CancellationToken.None);
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
            _events.Add(
                PushMessage.New(_state.ConnectionId, Response.Result(Request.Rid, responseResult.Join(result))),
                _cts.Token
            );
        }

        public void Send(TMessage message)
        {
            if (!_isInitiated)
                throw new InvalidOperationException("Can't send message from not initiated subscription");
            if (_cts.IsCancellationRequested)
                throw new InvalidOperationException("Can't send message from canceled subscription");

            _events.Add(
                PushMessage.New(_state.ConnectionId, new SubscriptionMessage<TMessage>(SubscriptionId, message)),
                _cts.Token
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

        public async ValueTask DisposeAsync()
        {
            _cts.Dispose();
            await _eventSenderTask;
        }
    }
}