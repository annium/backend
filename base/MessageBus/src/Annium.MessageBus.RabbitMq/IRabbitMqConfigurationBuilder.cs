namespace Annium.MessageBus.RabbitMq;

/// <summary>
/// Fluent builder for <see cref="RabbitMqConfiguration"/>, passed to <c>AddRabbitMqMessageBus</c>.
/// </summary>
public interface IRabbitMqConfigurationBuilder
{
    /// <summary>
    /// Sets the AMQP connection URI (<c>amqp://user:pass@host:port/vhost</c>). Only the <c>amqp</c>/<c>amqps</c>
    /// schemes are accepted.
    /// </summary>
    /// <param name="uri">The connection URI.</param>
    /// <returns>The builder for method chaining.</returns>
    IRabbitMqConfigurationBuilder ConnectionUri(string uri);

    /// <summary>
    /// Overrides the topic exchange name (defaults to <c>annium.messagebus</c>).
    /// </summary>
    /// <param name="exchange">The exchange name.</param>
    /// <returns>The builder for method chaining.</returns>
    IRabbitMqConfigurationBuilder Exchange(string exchange);
}
