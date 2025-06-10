using System;

namespace Annium.Infrastructure.MessageBus.Node.Internal;

/// <summary>
/// Implementation of IMessageBusRequestContext that manages request handling and response writing.
/// </summary>
/// <typeparam name="TRequest">The type of the request object.</typeparam>
/// <typeparam name="TResponse">The type of the response object.</typeparam>
internal class MessageBusRequestContext<TRequest, TResponse> : IMessageBusRequestContext<TRequest, TResponse>
{
    /// <summary>
    /// Gets the request object received from the MessageBus.
    /// </summary>
    public TRequest Request { get; }

    /// <summary>
    /// Gets a value indicating whether a response has already been written for this request.
    /// </summary>
    public bool IsResponded { get; private set; }

    /// <summary>
    /// Gets the response that was written for this request.
    /// </summary>
    internal TResponse Response { get; private set; } = default!;

    /// <summary>
    /// Initializes a new instance of the MessageBusRequestContext class.
    /// </summary>
    /// <param name="request">The request object to wrap in this context.</param>
    public MessageBusRequestContext(TRequest request)
    {
        Request = request;
    }

    /// <summary>
    /// Writes a response for the current request.
    /// </summary>
    /// <param name="response">The response object to send back.</param>
    public void WriteResponse(TResponse response)
    {
        if (IsResponded)
            throw new InvalidOperationException("Response already written");

        IsResponded = true;
        Response = response;
    }
}
