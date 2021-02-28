using System;
using System.Net.WebSockets;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Mediator;
using Annium.Infrastructure.WebSockets.Domain.Requests;
using Annium.Infrastructure.WebSockets.Domain.Responses;
using Annium.Infrastructure.WebSockets.Server.Internal.Models;
using Annium.Infrastructure.WebSockets.Server.Internal.Responses;
using Annium.Net.WebSockets;

namespace Annium.Infrastructure.WebSockets.Server.Internal
{
    internal class ConnectionHandler
    {
        private readonly IMediator _mediator;
        private readonly Serializer _serializer;
        private readonly WorkScheduler _scheduler;
        private readonly Connection _cn;
        private readonly ConnectionState _state;

        public ConnectionHandler(
            IMediator mediator,
            Serializer serializer,
            WorkScheduler scheduler,
            Connection cn
        )
        {
            _mediator = mediator;
            _serializer = serializer;
            _scheduler = scheduler;
            _cn = cn;
            _state = new ConnectionState(cn.Id);
        }

        public async Task HandleAsync(CancellationToken ct)
        {
            var tcs = new TaskCompletionSource();
            ct.Register(() => tcs.TrySetResult());
            _cn.Socket
                .Listen()
                .Subscribe(
                    x => _scheduler.Add(() => HandleMessage(x)),
                    () => tcs.SetResult(),
                    ct
                );
            await tcs.Task;
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
            if (msg.Type != WebSocketMessageType.Binary)
                return default;

            try
            {
                return _serializer.Deserialize<AbstractRequestBase>(msg.Data);
            }
            catch
            {
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
            await _cn.Socket.Send(_serializer.Serialize(response), CancellationToken.None);
        }
    }
}