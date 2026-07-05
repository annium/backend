using System.Collections.Generic;
using System.Threading.Tasks;

namespace Annium.MessageBus.Abstractions;

/// <summary>
/// A single raw message received from the transport, exposing the broker-level completion hooks the shared pipeline
/// drives once the handler has run. Adapters implement this over their native primitives (Kafka offset store,
/// RabbitMQ basic-ack/nack, NATS ack).
/// </summary>
public interface ITransportIncomingMessage
{
    /// <summary>
    /// Gets the canonical subject the message was received on.
    /// </summary>
    string Subject { get; }

    /// <summary>
    /// Gets the serialized payload.
    /// </summary>
    string Body { get; }

    /// <summary>
    /// Gets the canonical envelope + user headers.
    /// </summary>
    IReadOnlyDictionary<string, string> Headers { get; }

    /// <summary>
    /// Acknowledges/commits the message at the transport level, marking it as successfully consumed.
    /// </summary>
    /// <returns>A task that completes when the acknowledgement is recorded.</returns>
    Task CompleteAsync();

    /// <summary>
    /// Leaves the message unacknowledged so the transport redelivers it as-is (raw redelivery). Used when the
    /// handler faults without an explicit disposition; the retry policy is deliberately not engaged.
    /// </summary>
    /// <returns>A task that completes when the abandonment is recorded.</returns>
    Task AbandonAsync();
}
