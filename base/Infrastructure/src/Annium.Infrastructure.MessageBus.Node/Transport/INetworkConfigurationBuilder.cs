using Annium.Serialization.Abstractions;

namespace Annium.Infrastructure.MessageBus.Node.Transport;

/// <summary>
/// Defines a builder interface for configuring network-based MessageBus transport.
/// </summary>
public interface INetworkConfigurationBuilder
{
    /// <summary>
    /// Configures the network endpoints for publisher and subscriber connections.
    /// </summary>
    /// <param name="endpointsConfiguration">The endpoints configuration containing publisher and subscriber addresses.</param>
    /// <returns>The configuration builder instance for method chaining.</returns>
    INetworkConfigurationBuilder WithEndpoints(EndpointsConfiguration endpointsConfiguration);

    /// <summary>
    /// Configures the serializer to use for message serialization and deserialization.
    /// </summary>
    /// <param name="serializer">The string serializer to use for messages.</param>
    /// <returns>The configuration builder instance for method chaining.</returns>
    INetworkConfigurationBuilder WithSerializer(ISerializer<string> serializer);
}
