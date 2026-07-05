using System;

namespace Annium.MessageBus.Abstractions;

/// <summary>
/// Settings for a subscription.
/// </summary>
public record SubscriptionOptions
{
    /// <summary>
    /// Gets the subject to subscribe to. May contain canonical wildcards (<c>*</c> single token, <c>&gt;</c> tail)
    /// where the transport supports them.
    /// </summary>
    public required string Subject { get; init; }

    /// <summary>
    /// Gets the consumer group. When set, subscribers sharing it compete for messages (load sharing);
    /// when null, each subscriber receives every message (fan-out).
    /// </summary>
    public string? Group { get; init; }

    /// <summary>
    /// Gets the delivery guarantee. Defaults to <see cref="DeliveryMode.AtLeastOnce"/>.
    /// </summary>
    public DeliveryMode Delivery { get; init; } = DeliveryMode.AtLeastOnce;

    /// <summary>
    /// Gets the delivery window — the maximum number of in-flight (unacknowledged) messages. Flow control.
    /// </summary>
    public int Prefetch { get; init; } = 1;

    /// <summary>
    /// Gets the maximum number of handlers invoked concurrently. A value of 1 preserves order within the
    /// consumed unit. Must not exceed <see cref="Prefetch"/>.
    /// </summary>
    public int Concurrency { get; init; } = 1;

    /// <summary>
    /// Gets the retry policy applied before dead-lettering a failed message.
    /// </summary>
    public RetryPolicy Retry { get; init; } = RetryPolicy.Default;

    /// <summary>
    /// Gets the graceful drain timeout awaited on dispose for in-flight handlers to complete.
    /// </summary>
    public TimeSpan StopTimeout { get; init; } = TimeSpan.FromSeconds(30);
}
