using System;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Data.Operations;

namespace Annium.Infrastructure.MessageBus.Node.Internal;

/// <summary>
/// Implementation of IMessageBusClient that handles request-response communication through the MessageBus.
/// </summary>
internal class MessageBusClient : IMessageBusClient
{
    /// <summary>
    /// The MessageBus node used for communication.
    /// </summary>
    private readonly IMessageBusNode _node;

    /// <summary>
    /// Initializes a new instance of the MessageBusClient class.
    /// </summary>
    /// <param name="node">The MessageBus node to use for communication.</param>
    public MessageBusClient(IMessageBusNode node)
    {
        _node = node;
    }

    /// <summary>
    /// Sends a request to the specified topic and asynchronously waits for a response.
    /// </summary>
    /// <typeparam name="T">The type of the expected response.</typeparam>
    /// <param name="topic">The message topic to send the request to.</param>
    /// <param name="request">The request object to send.</param>
    /// <param name="ct">The cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation that returns the response result.</returns>
    public async Task<IResult<T>> FetchAsync<T>(string topic, object request, CancellationToken ct = default)
    {
        var id = Guid.NewGuid();

        var tcs = new TaskCompletionSource<IResult<T>>();
        ct.Register(() => tcs.TrySetCanceled());
        _node
            .Listen<MessageBusEnvelope<T>>()
            .Where(x => x.Id == id)
            .Subscribe(
                x =>
                {
                    if (x.IsSuccess)
                        tcs.TrySetResult(Result.New(x.Data));
                    else
                        tcs.TrySetException(new Exception(x.Error));
                },
                e => tcs.TrySetException(e),
                ct
            );

        await _node.Send(MessageBusEnvelope.Data(id, request));

        var result = await tcs.Task;

        return result;
    }
}
