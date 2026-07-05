using System;
using System.Collections.Generic;

namespace Annium.Infrastructure.MessageBus.Abstractions;

/// <summary>
/// The context of a single received message. Exactly one of <see cref="Ack"/> / <see cref="Nack"/> must be called
/// per message (on all paths, including exceptions). Failing to do so, or calling more than once, is a contract
/// violation. Both only record the intended disposition; the pipeline performs the transport-level ack/commit after
/// the handler returns, so neither is asynchronous.
/// </summary>
/// <typeparam name="T">The deserialized message payload type.</typeparam>
public interface IMessageContext<out T>
{
    /// <summary>
    /// Gets the message identifier (used for idempotency/tracing). Auto-generated on publish if not supplied.
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Gets the message headers.
    /// </summary>
    IReadOnlyDictionary<string, string> Headers { get; }

    /// <summary>
    /// Gets the publication timestamp.
    /// </summary>
    DateTimeOffset Timestamp { get; }

    /// <summary>
    /// Gets the deserialized message payload.
    /// </summary>
    T Payload { get; }

    /// <summary>
    /// Acknowledges successful processing (records the intent to commit/ack).
    /// </summary>
    void Ack();

    /// <summary>
    /// Rejects the message. When <paramref name="requeue"/> is true the message is retriable (retry policy,
    /// then dead-letter); when false it is dead-lettered immediately.
    /// </summary>
    /// <param name="requeue">Whether the failure is retriable.</param>
    void Nack(bool requeue = true);
}
