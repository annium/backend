using System.Threading;
using System.Threading.Tasks;
using Annium.Data.Operations;

namespace Annium.Infrastructure.MessageBus.Node;

/// <summary>
/// Defines a client interface for sending requests and receiving responses through the MessageBus.
/// </summary>
public interface IMessageBusClient
{
    /// <summary>
    /// Sends a request to the specified topic and asynchronously waits for a response.
    /// </summary>
    /// <typeparam name="T">The type of the expected response.</typeparam>
    /// <param name="topic">The message topic to send the request to.</param>
    /// <param name="request">The request object to send.</param>
    /// <param name="ct">The cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation that returns the response result.</returns>
    Task<IResult<T>> FetchAsync<T>(string topic, object request, CancellationToken ct = default);
}
