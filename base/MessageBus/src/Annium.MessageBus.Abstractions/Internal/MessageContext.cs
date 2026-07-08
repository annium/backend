using System;
using System.Collections.Generic;

namespace Annium.MessageBus.Abstractions.Internal;

/// <summary>
/// The pipeline's <see cref="IMessageContext{T}"/> implementation for a single processing attempt. Ack/Nack only
/// record the intended <see cref="Disposition"/> (with a strict single-call guard); the pipeline performs the
/// actual transport action after the handler returns. A fresh instance is created per retry attempt.
/// </summary>
/// <typeparam name="T">The deserialized message payload type.</typeparam>
internal sealed class MessageContext<T> : IMessageContext<T>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MessageContext{T}"/> class.
    /// </summary>
    /// <param name="id">The message identifier.</param>
    /// <param name="headers">The message headers.</param>
    /// <param name="timestamp">The publication timestamp.</param>
    /// <param name="payload">The deserialized payload.</param>
    public MessageContext(string id, IReadOnlyDictionary<string, string> headers, DateTimeOffset timestamp, T payload)
    {
        Id = id;
        Headers = headers;
        Timestamp = timestamp;
        Body = payload;
    }

    /// <inheritdoc />
    public string Id { get; }

    /// <inheritdoc />
    public IReadOnlyDictionary<string, string> Headers { get; }

    /// <inheritdoc />
    public DateTimeOffset Timestamp { get; }

    /// <inheritdoc />
    public T Body { get; }

    /// <summary>
    /// Gets the recorded disposition for this attempt.
    /// </summary>
    public Disposition Disposition { get; private set; }

    /// <summary>
    /// Gets a value indicating whether a Nack requested requeue (retriable). Only meaningful when
    /// <see cref="Disposition"/> is <see cref="Disposition.Nack"/>.
    /// </summary>
    public bool NackRequeue { get; private set; }

    /// <inheritdoc />
    public void Ack()
    {
        EnsureUndecided();
        Disposition = Disposition.Ack;
    }

    /// <inheritdoc />
    public void Nack(bool requeue = true)
    {
        EnsureUndecided();
        Disposition = Disposition.Nack;
        NackRequeue = requeue;
    }

    /// <summary>
    /// Throws if a disposition has already been recorded for this attempt.
    /// </summary>
    private void EnsureUndecided()
    {
        if (Disposition != Disposition.None)
            throw new InvalidOperationException(
                $"Message '{Id}' was already {Disposition.ToString().ToLowerInvariant()}ed; ack/nack must be called exactly once."
            );
    }
}
