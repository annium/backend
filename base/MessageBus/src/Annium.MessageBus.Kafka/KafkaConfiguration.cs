using System.Collections.Generic;

namespace Annium.MessageBus.Kafka;

/// <summary>
/// The resolved Kafka adapter configuration built by <see cref="IKafkaConfigurationBuilder"/>. A plain DTO holding the
/// parsed bootstrap endpoints (validation/parsing lives in the builder).
/// </summary>
public sealed record KafkaConfiguration
{
    /// <summary>
    /// Gets the parsed Kafka bootstrap servers.
    /// </summary>
    public required IReadOnlyList<KafkaEndpoint> BootstrapServers { get; init; }
}
