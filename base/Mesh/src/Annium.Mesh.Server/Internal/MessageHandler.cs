using System;
using System.Threading.Tasks;
using Annium.Core.Mediator;
using Annium.Logging;
using Annium.Mesh.Domain.Requests;
using Annium.Mesh.Domain.Responses;
using Annium.Mesh.Server.Internal.Models;
using Annium.Mesh.Server.Internal.Responses;
using Annium.Mesh.Server.Internal.Serialization;
using Annium.Mesh.Server.Models;
using Annium.Mesh.Transport.Abstractions;

namespace Annium.Mesh.Server.Internal;

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

    public async Task HandleMessage(ISendingConnection connection, TState state, AbstractRequestBase request)
    {
        var response = await ProcessRequest(state, request);
        await SendResponse(connection, response);

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

    private async Task SendResponse(ISendingConnection connection, AbstractResponseBase response)
    {
        switch (response)
        {
            case IVoidResponse:
                break;
            case IMetaResponse meta:
                await SendInternal(connection, meta.Response);
                break;
            default:
                await SendInternal(connection, response);
                break;
        }
    }

    private async Task SendInternal(ISendingConnection connection, AbstractResponseBase response)
    {
        try
        {
            this.Trace("Send response {responseType}#{responseId}", response.Tid, response is ResponseBase res ? res.Rid : Guid.Empty);
            await connection.SendAsync(_serializer.Serialize(response));
        }
        catch (Exception e)
        {
            this.Error(e);
            throw;
        }
    }
}