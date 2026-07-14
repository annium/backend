namespace Annium.MessageBus.Abstractions;

/// <summary>
/// A single message delivered by an <see cref="ITransportConsumer"/>: the message data plus an optional opaque token
/// the adapter uses to correlate acknowledgement with its native handle (e.g. Kafka offset, RabbitMQ delivery tag,
/// NATS message). The pipeline treats <see cref="Token"/> as opaque and hands the delivery back to the consumer's
/// <see cref="ITransportConsumer.CompleteAsync"/> / <see cref="ITransportConsumer.AbandonAsync"/>.
/// </summary>
/// <param name="Message">The message data.</param>
/// <param name="Token">The adapter's opaque acknowledgement handle, or null when not needed (e.g. in-memory).</param>
public readonly record struct TransportDelivery(TransportMessage Message, object? Token = null);
