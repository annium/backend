using System;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Core.Internal;
using Annium.Core.Mediator;
using Annium.Extensions.Execution;
using Annium.Infrastructure.WebSockets.Domain.Models;
using Annium.Infrastructure.WebSockets.Domain.Requests;
using Annium.Infrastructure.WebSockets.Domain.Responses;
using Annium.Infrastructure.WebSockets.Server.Internal.Models;
using Annium.Infrastructure.WebSockets.Server.Internal.Responses;
using Annium.Infrastructure.WebSockets.Server.Internal.Serialization;
using Annium.Net.WebSockets;

namespace Annium.Infrastructure.WebSockets.Server.Internal
{
    internal class ConnectionHandler<TState> : IAsyncDisposable
        where TState : ConnectionStateBase
    {
        private readonly IServiceProvider _sp;
        private readonly IMediator _mediator;
        private readonly Serializer _serializer;
        private readonly Connection _cn;
        private readonly TState _state;

        public ConnectionHandler(
            IServiceProvider sp,
            IMediator mediator,
            Serializer serializer,
            Func<Guid, TState> stateFactory,
            Connection cn
        )
        {
            _sp = sp;
            _mediator = mediator;
            _serializer = serializer;
            _cn = cn;
            _state = stateFactory(cn.Id);
        }

        public async Task HandleAsync(CancellationToken ct)
        {
            this.Trace(() => "Start");
            await using var scope = _sp.CreateAsyncScope();
            var executor = Executor.Background.Parallel();
            LifeCycleCoordinator<TState>? lifeCycleCoordinator = null;
            try
            {
                lifeCycleCoordinator = scope.ServiceProvider.Resolve<LifeCycleCoordinator<TState>>();
                var tcs = new TaskCompletionSource<object>();

                // immediately subscribe to cancellation
                ct.Register(() => tcs.TrySetResult(new object()));

                // start listening to messages and adding them to scheduler
                this.Trace(() => "Init subscription");
                _cn.Socket
                    .Listen()
                    .Subscribe(
                        x => executor.Schedule(() => HandleMessage(x)),
                        x => tcs.TrySetException(x),
                        () => tcs.TrySetResult(new object()),
                        ct
                    );

                // process start hook
                await lifeCycleCoordinator.HandleStartAsync(_state);

                // start scheduler to process backlog and run upcoming work immediately
                this.Trace(() => "Start executor");
                executor.Start(CancellationToken.None);

                // wait until connection complete
                this.Trace(() => "Wait until connection complete");
                await tcs.Task;
            }
            finally
            {
                // all handlers must be complete before teardown lifecycle hook
                this.Trace(() => "Dispose executor");
                await executor.DisposeAsync();

                // process end hook
                if (lifeCycleCoordinator is not null)
                    await lifeCycleCoordinator.HandleEndAsync(_state);
            }
        }

        public async ValueTask DisposeAsync()
        {
            await _state.DisposeAsync();
        }

        private async Task HandleMessage(SocketMessage msg)
        {
            var request = ParseRequest(msg);
            if (request is null)
                return;

            var response = await ProcessRequest(request);
            await SendResponse(response);

            // TODO: implementation. Request/response streams should rely on modified IMediator implementation
            // request (and it's chunks, and error!) is sent via IMediator
            // just before exact handler, sits STATEFUL! request tracker, that:
            // works with Container<T> - object with request ID, identifying it in stateful environment
            // for requests:
            //  - creates request objects
            //  - passes them down to handler
            //  - tracks request chunks (IDictionary<RequestId,Request> and passes them inside IObservable request along with End event
            // for responses:
            //  - receives notification from IObservable response object
            //  - invokes IMediator BACK! to Coordinator
            //  - Coordinator's generic handler method identifies connection by request id, serializes and sends response
            // corner cases:
            //  - it's up to exact handler to handle request error (if needed)
            //  - Coordinator for now will have configurable default policy to terminate requests, not closed in 1 minute since last chunk
            //  - when request is terminated, error is propagated to handler, so it can handle this (remove allocated resources, log exception, etc)
            //  - Coordinator for now will have configurable default policy to terminate responses, not closed in 1 minute since last chunk
            //  - when response is terminated, response message is sent to receiver with close marker and error message, so it can handle it appropriately
        }

        private AbstractRequestBase? ParseRequest(SocketMessage msg)
        {
            try
            {
                return _serializer.Deserialize<AbstractRequestBase>(msg.Data);
            }
            catch (Exception e)
            {
                this.Trace(e.ToString);
                return default;
            }
        }

        private async Task<AbstractResponseBase> ProcessRequest(AbstractRequestBase request)
        {
            var context = RequestContext.CreateDynamic(request, _state);
            return await _mediator.SendAsync<AbstractResponseBase>(context);
        }

        private async Task SendResponse(AbstractResponseBase response)
        {
            switch (response)
            {
                case IVoidResponse:
                    break;
                case IMetaResponse meta:
                    await SendInternal(meta.Response);
                    break;
                default:
                    await SendInternal(response);
                    break;
            }
        }

        private async Task SendInternal(AbstractResponseBase response)
        {
            await _cn.Socket.SendWith(response, _serializer, CancellationToken.None);
        }
    }
}