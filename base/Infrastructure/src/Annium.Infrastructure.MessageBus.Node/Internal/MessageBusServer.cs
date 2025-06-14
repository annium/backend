using System;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Annium.Infrastructure.MessageBus.Node.Internal;

/// <summary>
/// Implementation of IMessageBusServer that handles request processing and response sending.
/// </summary>
internal class MessageBusServer : IMessageBusServer
{
    /// <summary>
    /// The MessageBus node used for communication.
    /// </summary>
    private readonly IMessageBusNode _node;

    /// <summary>
    /// Initializes a new instance of the MessageBusServer class.
    /// </summary>
    /// <param name="node">The MessageBus node to use for communication.</param>
    public MessageBusServer(IMessageBusNode node)
    {
        _node = node;
    }

    /// <summary>
    /// Registers a handler for processing requests of specific types on a given topic.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request to handle.</typeparam>
    /// <typeparam name="TResponse">The type of the response to return.</typeparam>
    /// <param name="topic">The message topic to handle requests for.</param>
    /// <param name="process">The asynchronous function to process incoming requests.</param>
    /// <returns>A disposable that can be used to unregister the handler.</returns>
    public IDisposable Handle<TRequest, TResponse>(
        string topic,
        Func<IMessageBusRequestContext<TRequest, TResponse>, Task> process
    ) =>
        _node
            .Listen<MessageBusEnvelope<TRequest>>()
            .DoSequentialAsync(async x =>
            {
                try
                {
                    var ctx = new MessageBusRequestContext<TRequest, TResponse>(x.Data);
                    await process(ctx);
                    if (ctx.IsResponded)
                        await _node.Send(MessageBusEnvelope.Data(x.Id, ctx.Response));
                }
                catch (Exception e)
                {
                    await _node.Send(MessageBusEnvelope.Error(x.Id, e.Message));
                }
            })
            .Subscribe();
}
