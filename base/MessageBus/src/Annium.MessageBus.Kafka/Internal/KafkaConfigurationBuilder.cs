using System;
using System.Collections.Generic;

namespace Annium.MessageBus.Kafka.Internal;

/// <summary>
/// The default <see cref="IKafkaConfigurationBuilder"/> implementation. Parses and validates input eagerly (via
/// <see cref="BootstrapServersParser"/>), holding the parsed endpoints, and builds an immutable
/// <see cref="KafkaConfiguration"/>.
/// </summary>
internal sealed class KafkaConfigurationBuilder : IKafkaConfigurationBuilder
{
    /// <summary>
    /// The parsed bootstrap endpoints, if configured.
    /// </summary>
    private IReadOnlyList<KafkaEndpoint>? _bootstrapServers;

    /// <summary>
    /// Sets the Kafka bootstrap servers (comma-separated <c>host:port</c> list).
    /// </summary>
    /// <param name="servers">The bootstrap servers.</param>
    /// <returns>The builder for method chaining.</returns>
    public IKafkaConfigurationBuilder BootstrapServers(string servers)
    {
        _bootstrapServers = BootstrapServersParser.Parse(servers);
        return this;
    }

    /// <summary>
    /// Builds the immutable configuration.
    /// </summary>
    /// <returns>The resolved configuration.</returns>
    /// <exception cref="InvalidOperationException">Thrown when bootstrap servers were not configured.</exception>
    public KafkaConfiguration Build()
    {
        if (_bootstrapServers is null || _bootstrapServers.Count == 0)
            throw new InvalidOperationException("Kafka bootstrap servers must be configured.");

        return new KafkaConfiguration { BootstrapServers = _bootstrapServers };
    }
}
