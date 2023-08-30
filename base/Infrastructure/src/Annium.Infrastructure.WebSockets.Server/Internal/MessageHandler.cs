using System;
using System.Threading.Tasks;
using Annium.Core.Mediator;
using Annium.Infrastructure.WebSockets.Domain.Requests;
using Annium.Infrastructure.WebSockets.Domain.Responses;
using Annium.Infrastructure.WebSockets.Server.Internal.Models;
using Annium.Infrastructure.WebSockets.Server.Internal.Responses;
using Annium.Infrastructure.WebSockets.Server.Internal.Serialization;
using Annium.Infrastructure.WebSockets.Server.Models;
using Annium.Logging;
using Annium.Net.WebSockets;

namespace Annium.Infrastructure.WebSockets.Server.Internal;

internal class MessageHandler<TState> : ILogSubject
    where TState : ConnectionStateBase
{
    public ILogger Logger { get; }
    private readonly IServiceProvider _sp;
    private readonly Serializer _serializer;
    private readonly IMediator _mediator;

    public MessageHandler(
        IServiceProvider sp,
        Serializer serializer,
        IMediator mediator,
        ILogger logger
    )
    {
        _sp = sp;
        _serializer = serializer;
        _mediator = mediator;
        Logger = logger;
    }

    public async Task HandleMessage(ISendingWebSocket socket, TState state, ReadOnlyMemory<byte> msg)
    {
        var request = ParseRequest(msg);
        if (request is null)
            return;

        var response = await ProcessRequest(state, request);
        await SendResponse(socket, response);

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

    private AbstractRequestBase? ParseRequest(ReadOnlyMemory<byte> msg)
    {
        try
        {
            return _serializer.Deserialize<AbstractRequestBase>(msg);
        }
        catch (Exception e)
        {
            this.Warn(e.ToString());
            return default;
        }
    }

    private async Task<AbstractResponseBase> ProcessRequest(TState state, AbstractRequestBase request)
    {
        try
        {
            this.Trace("Process request {requestType}#{requestId}", request.Tid, request.Rid);
            var context = RequestContext.CreateDynamic(request, state);
            return await _mediator.SendAsync<AbstractResponseBase>(_sp, context);
        }
        catch (Exception e)
        {
            this.Error(e);
            throw;
        }
    }

    private async Task SendResponse(ISendingWebSocket socket, AbstractResponseBase response)
    {
        switch (response)
        {
            case IVoidResponse:
                break;
            case IMetaResponse meta:
                await SendInternal(socket, meta.Response);
                break;
            default:
                await SendInternal(socket, response);
                break;
        }
    }

    private async Task SendInternal(ISendingWebSocket socket, AbstractResponseBase response)
    {
        try
        {
            this.Trace("Send response {responseType}#{responseId}", response.Tid, response is ResponseBase res ? res.Rid : Guid.Empty);
            await socket.SendBinaryAsync(_serializer.Serialize(new ConnectionReadyNotification()));
        }
        catch (Exception e)
        {
            this.Error(e);
            throw;
        }
    }
}