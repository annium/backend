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

    /// <summary>
    /// Gets the message identifier (used for idempotency/tracing). Auto-generated on publish if not supplied.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets the message headers.
    /// </summary>
    public IReadOnlyDictionary<string, string> Headers { get; }

    /// <summary>
    /// Gets the publication timestamp.
    /// </summary>
    public DateTimeOffset Timestamp { get; }

    /// <summary>
    /// Gets the deserialized message payload.
    /// </summary>
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

    /// <summary>
    /// Acknowledges successful processing (records the intent to commit/ack).
    /// </summary>
    public void Ack()
    {
        EnsureUndecided();
        Disposition = Disposition.Ack;
    }

    /// <summary>
    /// Rejects the message. When <paramref name="requeue"/> is true the message is retriable (retry policy,
    /// then dead-letter); when false it is dead-lettered immediately.
    /// </summary>
    /// <param name="requeue">Whether the failure is retriable.</param>
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
