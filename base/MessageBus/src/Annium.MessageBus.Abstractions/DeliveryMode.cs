namespace Annium.MessageBus.Abstractions;

/// <summary>
/// Delivery guarantee for publish and subscribe operations. Exactly-once is intentionally not offered —
/// idempotency/deduplication is the application's responsibility.
/// </summary>
public enum DeliveryMode
{
    /// <summary>
    /// At-most-once: messages may be lost but are never redelivered. Fire-and-forget.
    /// </summary>
    AtMostOnce,

    /// <summary>
    /// At-least-once: messages are redelivered until acknowledged; duplicates are possible.
    /// </summary>
    AtLeastOnce,
}
