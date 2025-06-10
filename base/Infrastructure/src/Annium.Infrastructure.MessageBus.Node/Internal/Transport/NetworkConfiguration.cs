using Annium.Serialization.Abstractions;

namespace Annium.Infrastructure.MessageBus.Node.Internal.Transport;

/// <summary>
/// Configuration implementation for network-based MessageBus transport using NetMQ.
/// </summary>
internal class NetworkConfiguration : IConfiguration
{
    /// <summary>
    /// Gets the network endpoints configuration for publisher and subscriber connections.
    /// </summary>
    public EndpointsConfiguration Endpoints { get; }

    /// <summary>
    /// Gets the serializer used for message serialization and deserialization.
    /// </summary>
    public ISerializer<string> Serializer { get; }

    /// <summary>
    /// Initializes a new instance of the NetworkConfiguration class.
    /// </summary>
    /// <param name="endpoints">The endpoints configuration for network connections.</param>
    /// <param name="serializer">The serializer to use for messages.</param>
    public NetworkConfiguration(EndpointsConfiguration endpoints, ISerializer<string> serializer)
    {
        Endpoints = endpoints;
        Serializer = serializer;
    }
}
