using System;

namespace Annium.MessageBus.RabbitMq;

/// <summary>
/// The RabbitMQ adapter configuration: the AMQP connection endpoint and the topic exchange all subjects route through.
/// Built by <see cref="IRabbitMqConfigurationBuilder"/> and passed to <c>AddRabbitMqMessageBus</c>.
/// </summary>
public sealed record RabbitMqConfiguration
{
    /// <summary>
    /// Gets the AMQP connection URI (<c>amqp://user:pass@host:port/vhost</c>).
    /// </summary>
    public required Uri ConnectionUri { get; init; }

    /// <summary>
    /// Gets the name of the durable topic exchange all subjects are published to and bound against.
    /// </summary>
    public required string ExchangeName { get; init; }
}
