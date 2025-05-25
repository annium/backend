using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Logging;
using Annium.Mesh.Domain;
using Annium.Mesh.Serialization.Abstractions;
using Annium.Mesh.Server.Components;
using Annium.Mesh.Server.Internal.Components;
using Annium.Mesh.Server.Internal.Routing;
using Annium.Mesh.Transport.Abstractions;

namespace Annium.Mesh.Server.Internal;

internal class MessageHandler : ILogSubject
{
    public ILogger Logger { get; }
    private readonly IServiceProvider _sp;
    private readonly RouteStore _routeStore;
    private readonly ISerializer _serializer;
    private readonly IMessageSender _sender;

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

    public Task HandleMessageAsync(Guid cid, ISendingConnection cn, Message message, CancellationToken ct) =>
        message.Type switch
        {
            MessageType.Request => HandleRequestAsync(cid, cn, message, ct),
            MessageType.Event => HandleEventAsync(),
            MessageType.SubscriptionInit => HandleSubscriptionInitAsync(),
            MessageType.SubscriptionCancel => HandleSubscriptionCancelAsync(),
            _ => Task.CompletedTask,
        };

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

    private async Task HandleEventAsync()
    {
        this.Trace("ignore");
        await Task.CompletedTask;
    }

    private async Task HandleSubscriptionInitAsync()
    {
        this.Trace("ignore");
        await Task.CompletedTask;
    }

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
