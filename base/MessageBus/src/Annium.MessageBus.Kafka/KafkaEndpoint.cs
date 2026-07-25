namespace Annium.MessageBus.Kafka;

/// <summary>
/// A single Kafka bootstrap server: a host and a port.
/// </summary>
/// <param name="Host">The broker host.</param>
/// <param name="Port">The broker port (1-65535).</param>
public readonly record struct KafkaEndpoint(string Host, int Port)
{
    /// <summary>
    /// Returns the canonical <c>host:port</c> representation.
    /// </summary>
    /// <returns>The <c>host:port</c> string.</returns>
    public override string ToString() => $"{Host}:{Port}";
}
