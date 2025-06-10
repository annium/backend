using Annium.Serialization.Abstractions;

namespace Annium.Infrastructure.MessageBus.Node.Internal.Transport;

/// <summary>
/// Defines the base configuration interface for MessageBus transport implementations.
/// </summary>
internal interface IConfiguration
{
    /// <summary>
    /// Gets the serializer used for message serialization and deserialization.
    /// </summary>
    ISerializer<string> Serializer { get; }
}
