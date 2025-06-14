using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Logging;
using Annium.Mesh.Domain;
using Annium.Mesh.Serialization.Abstractions;
using Annium.Mesh.Server.Internal.Components;
using Annium.Mesh.Server.Internal.Routing;
using Annium.Mesh.Transport.Abstractions;

namespace Annium.Mesh.Server.Internal;

/// <summary>
/// Handles incoming messages from clients by routing them to appropriate handlers based on message type and action.
/// </summary>
internal class MessageHandler : ILogSubject
{
    /// <summary>
    /// Gets the logger for this message handler.
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// The service provider for resolving handler instances.
    /// </summary>
    private readonly IServiceProvider _sp;

    /// <summary>
    /// The route store containing registered message routes.
    /// </summary>
    private readonly RouteStore _routeStore;

    /// <summary>
    /// The serializer for message data.
    /// </summary>
    private readonly ISerializer _serializer;

    /// <summary>
    /// The message sender for sending responses back to clients.
    /// </summary>
    private readonly IMessageSender _sender;

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageHandler"/> class.
    /// </summary>
    /// <param name="sp">The service provider for resolving handler instances.</param>
    /// <param name="routeStore">The route store containing registered message routes.</param>
    /// <param name="serializer">The serializer for message data.</param>
    /// <param name="sender">The message sender for sending responses.</param>
    /// <param name="logger">The logger for this message handler.</param>
    public MessageHandler(
        IServiceProvider sp,
        RouteStore routeStore,
        ISerializer serializer,
        IMessageSender sender,
        ILogger logger
    )
    {
        Logger = logger;
        _sp = sp;
        _routeStore = routeStore;
        _serializer = serializer;
        _sender = sender;
    }

    /// <summary>
    /// Handles an incoming message by routing it to the appropriate handler based on message type.
    /// </summary>
    /// <param name="cid">The connection identifier.</param>
    /// <param name="cn">The sending connection for responses.</param>
    /// <param name="message">The message to handle.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous message handling operation.</returns>
    public Task HandleMessageAsync(Guid cid, ISendingConnection cn, Message message, CancellationToken ct) =>
        message.Type switch
        {
            MessageType.Request => HandleRequestAsync(cid, cn, message, ct),
            MessageType.Event => HandleEventAsync(),
            MessageType.SubscriptionInit => HandleSubscriptionInitAsync(),
            MessageType.SubscriptionCancel => HandleSubscriptionCancelAsync(),
            _ => Task.CompletedTask,
        };

    /// <summary>
    /// Handles a request message by finding the appropriate handler, executing it, and sending the response.
    /// </summary>
    /// <param name="cid">The connection identifier.</param>
    /// <param name="cn">The sending connection for the response.</param>
    /// <param name="message">The request message to handle.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous request handling operation.</returns>
    private async Task HandleRequestAsync(Guid cid, ISendingConnection cn, Message message, CancellationToken ct)
    {
        this.Trace("start");

        var actionKey = new ActionKey(message.Version, message.Action);
        if (!_routeStore.RequestRoutes.TryGet(actionKey, out var route))
        {
            this.Warn("no route registered for {key}", actionKey);
            return;
        }

        var request = _serializer.DeserializeData(route.RequestType, message.Data);
        if (request is null)
        {
            this.Error("failed to parse {type} request from {message}", route.RequestType.FriendlyName(), message);
            return;
        }

        this.Trace<string>("resolve handler {handlerType}", route.HandlerType.FriendlyName());
        var handler = _sp.Resolve(route.HandlerType);

        // execute and resolve result data
        this.Trace<string>("execute handler {handlerType}", route.HandlerType.FriendlyName());
        var resultTask = route.HandleMethod.Invoke(handler, new[] { request, ct })!;
        await (Task)resultTask;

        var dataType = route.ResultProperty.PropertyType;
        var data = route.ResultProperty.GetValue(resultTask).NotNull();

        this.Trace("send response {data} to client", data);
        await _sender.SendAsync(cid, cn, message.Id, actionKey, MessageType.Response, dataType, data, ct);

        this.Trace("done");
    }

    /// <summary>
    /// Handles an event message. Currently events are ignored by the server.
    /// </summary>
    /// <returns>A completed task.</returns>
    private async Task HandleEventAsync()
    {
        this.Trace("ignore");
        await Task.CompletedTask;
    }

    /// <summary>
    /// Handles a subscription initialization message. Currently subscription init messages are ignored by the server.
    /// </summary>
    /// <returns>A completed task.</returns>
    private async Task HandleSubscriptionInitAsync()
    {
        this.Trace("ignore");
        await Task.CompletedTask;
    }

    /// <summary>
    /// Handles a subscription cancellation message. Currently subscription cancel messages are ignored by the server.
    /// </summary>
    /// <returns>A completed task.</returns>
    private async Task HandleSubscriptionCancelAsync()
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
