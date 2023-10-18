using System;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Mesh.Domain;
using Annium.Mesh.Serialization.Abstractions;
using Annium.Mesh.Transport.Abstractions;

namespace Annium.Mesh.Server.Internal;

internal class MessageHandler : ILogSubject
{
    public ILogger Logger { get; }
    private readonly ISerializer _serializer;

    public MessageHandler(
        ISerializer serializer,
        ILogger logger
    )
    {
        Logger = logger;
        _serializer = serializer;
    }

    public Task HandleMessage(ISendingConnection connection, Guid cid, Message message) => message.Type switch
    {
        MessageType.Request            => HandleRequest(connection, message),
        MessageType.Event              => HandleEvent(connection, message),
        MessageType.SubscriptionInit   => HandleSubscriptionInit(connection, message),
        MessageType.SubscriptionCancel => HandleSubscriptionCancel(connection, message),
        _                              => Task.CompletedTask,
    };

    private async Task HandleRequest(ISendingConnection connection, Message message)
    {
        this.Trace("ignore");
        await Task.CompletedTask;
    }

    private async Task HandleEvent(ISendingConnection connection, Message message)
    {
        this.Trace("ignore");
        await Task.CompletedTask;
    }

    private async Task HandleSubscriptionInit(ISendingConnection connection, Message message)
    {
        this.Trace("ignore");
        await Task.CompletedTask;
    }

    private async Task HandleSubscriptionCancel(ISendingConnection connection, Message message)
    {
        this.Trace("ignore");
        await Task.CompletedTask;
    }
    //
    // private async Task<AbstractResponseBaseObsolete> ProcessRequest(Guid cid, AbstractRequestBaseObsolete request)
    // {
    //     try
    //     {
    //         var context = RequestContext.CreateDynamic(cid, request);
    //         this.Trace<string, Guid, string>("Process request {requestType}#{requestId} with context {context}", request.Tid, request.Rid, context.GetFullId());
    //         return await _mediator.SendAsync<AbstractResponseBaseObsolete>(_sp, context);
    //     }
    //     catch (Exception e)
    //     {
    //         this.Error(e);
    //         throw;
    //     }
    // }
    //
    // private async Task SendResponse(ISendingConnection connection, AbstractResponseBaseObsolete response)
    // {
    //     switch (response)
    //     {
    //         case IVoidResponse:
    //             break;
    //         default:
    //             await SendInternal(connection, response);
    //             break;
    //     }
    // }
    //
    // private async Task SendInternal(ISendingConnection connection, AbstractResponseBaseObsolete response)
    // {
    //     try
    //     {
    //         this.Trace("Send response {responseType}#{responseId}", response.Tid, response is ResponseBaseObsolete res ? res.Rid : Guid.Empty);
    //         await connection.SendAsync(_serializer.Serialize(response));
    //     }
    //     catch (Exception e)
    //     {
    //         this.Error(e);
    //         throw;
    //     }
    // }
}