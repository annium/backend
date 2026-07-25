namespace Annium.MessageBus.Nats;

/// <summary>
/// Fluent builder for <see cref="NatsConfiguration"/>, passed to <c>AddNatsMessageBus</c>.
/// </summary>
public interface INatsConfigurationBuilder
{
    /// <summary>
    /// Sets the NATS server URL (<c>nats://host:port</c> or <c>tls://host:port</c>).
    /// </summary>
    /// <param name="url">The server URL.</param>
    /// <returns>The builder for method chaining.</returns>
    INatsConfigurationBuilder Url(string url);
}
