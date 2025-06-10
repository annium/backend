namespace Annium.Infrastructure.MessageBus.Node;

/// <summary>
/// Defines a context for handling MessageBus requests, providing access to the request and response functionality.
/// </summary>
/// <typeparam name="TRequest">The type of the request object.</typeparam>
/// <typeparam name="TResponse">The type of the response object.</typeparam>
public interface IMessageBusRequestContext<TRequest, TResponse>
{
    /// <summary>
    /// Gets the request object received from the MessageBus.
    /// </summary>
    TRequest Request { get; }

    /// <summary>
    /// Gets a value indicating whether a response has already been written for this request.
    /// </summary>
    bool IsResponded { get; }

    /// <summary>
    /// Writes a response for the current request.
    /// </summary>
    /// <param name="response">The response object to send back.</param>
    void WriteResponse(TResponse response);
}
