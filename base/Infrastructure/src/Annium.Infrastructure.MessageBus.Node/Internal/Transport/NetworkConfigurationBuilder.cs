using System;
using Annium.Infrastructure.MessageBus.Node.Transport;
using Annium.Serialization.Abstractions;

namespace Annium.Infrastructure.MessageBus.Node.Internal.Transport;

/// <summary>
/// Builder implementation for configuring network-based MessageBus transport.
/// </summary>
internal class NetworkConfigurationBuilder : INetworkConfigurationBuilder
{
    /// <summary>
    /// The endpoints configuration for network connections.
    /// </summary>
    private EndpointsConfiguration? _endpointsConfiguration;

    /// <summary>
    /// The serializer to use for message serialization.
    /// </summary>
    private ISerializer<string>? _serializer;

    /// <summary>
    /// Configures the network endpoints for publisher and subscriber connections.
    /// </summary>
    /// <param name="endpointsConfiguration">The endpoints configuration containing publisher and subscriber addresses.</param>
    /// <returns>The configuration builder instance for method chaining.</returns>
    public INetworkConfigurationBuilder WithEndpoints(EndpointsConfiguration endpointsConfiguration)
    {
        _endpointsConfiguration = endpointsConfiguration;

        return this;
    }

    /// <summary>
    /// Configures the serializer to use for message serialization and deserialization.
    /// </summary>
    /// <param name="serializer">The string serializer to use for messages.</param>
    /// <returns>The configuration builder instance for method chaining.</returns>
    public INetworkConfigurationBuilder WithSerializer(ISerializer<string> serializer)
    {
        _serializer = serializer;

        return this;
    }

    /// <summary>
    /// Builds the network configuration with the specified settings.
    /// </summary>
    /// <returns>The configured NetworkConfiguration instance.</returns>
    public NetworkConfiguration Build()
    {
        if (_endpointsConfiguration is null)
            throw new ArgumentException("MessageBus endpoints are not configured");

        if (_serializer is null)
            throw new ArgumentException("MessageBus serializer is not configured");

        return new NetworkConfiguration(_endpointsConfiguration, _serializer);
    }
}
