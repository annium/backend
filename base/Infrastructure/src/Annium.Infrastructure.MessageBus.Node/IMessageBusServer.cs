using System;
using System.Threading.Tasks;

namespace Annium.Infrastructure.MessageBus.Node;

/// <summary>
/// Defines a server interface for handling MessageBus requests and processing them asynchronously.
/// </summary>
public interface IMessageBusServer
{
    /// <summary>
    /// Registers a handler for processing requests of specific types on a given topic.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request to handle.</typeparam>
    /// <typeparam name="TResponse">The type of the response to return.</typeparam>
    /// <param name="topic">The message topic to handle requests for.</param>
    /// <param name="process">The asynchronous function to process incoming requests.</param>
    /// <returns>A disposable that can be used to unregister the handler.</returns>
    IDisposable Handle<TRequest, TResponse>(
        string topic,
        Func<IMessageBusRequestContext<TRequest, TResponse>, Task> process
    );
}
