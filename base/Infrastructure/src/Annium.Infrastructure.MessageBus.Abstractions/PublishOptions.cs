using System.Collections.Generic;

namespace Annium.Infrastructure.MessageBus.Abstractions;

/// <summary>
/// Optional per-publish settings.
/// </summary>
public sealed record PublishOptions
{
    /// <summary>
    /// Gets the ordering/partitioning key. Native ordering-by-key is guaranteed only where the transport
    /// supports it (Kafka partition key); on RabbitMQ it is best-effort, on NATS it does not affect ordering.
    /// </summary>
    public string? Key { get; init; }

    /// <summary>
    /// Gets user-defined headers carried alongside the message.
    /// </summary>
    public IReadOnlyDictionary<string, string>? Headers { get; init; }
}
