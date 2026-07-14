namespace Annium.MessageBus.Kafka;

/// <summary>
/// Fluent builder for <see cref="KafkaConfiguration"/>, passed to <c>AddKafkaMessageBus</c>.
/// </summary>
public interface IKafkaConfigurationBuilder
{
    /// <summary>
    /// Sets the Kafka bootstrap servers (comma-separated <c>host:port</c> list).
    /// </summary>
    /// <param name="servers">The bootstrap servers.</param>
    /// <returns>The builder for method chaining.</returns>
    IKafkaConfigurationBuilder BootstrapServers(string servers);
}
