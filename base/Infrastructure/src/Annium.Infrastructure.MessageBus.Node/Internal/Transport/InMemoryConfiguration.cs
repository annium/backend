using Annium.Serialization.Abstractions;

namespace Annium.Infrastructure.MessageBus.Node.Internal.Transport;

/// <summary>
/// Configuration implementation for in-memory MessageBus transport.
/// </summary>
internal class InMemoryConfiguration : IConfiguration
{
    /// <summary>
    /// Gets the serializer used for message serialization and deserialization.
    /// </summary>
    public ISerializer<string> Serializer { get; }

    /// <summary>
    /// Initializes a new instance of the InMemoryConfiguration class.
    /// </summary>
    /// <param name="serializer">The serializer to use for messages.</param>
    public InMemoryConfiguration(ISerializer<string> serializer)
    {
        Serializer = serializer;
    }
}
