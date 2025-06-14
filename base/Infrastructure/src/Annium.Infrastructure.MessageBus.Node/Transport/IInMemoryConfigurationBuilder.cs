using Annium.Serialization.Abstractions;

// ReSharper disable once CheckNamespace
namespace Annium.Infrastructure.MessageBus.Node;

/// <summary>
/// Defines a builder interface for configuring in-memory MessageBus transport.
/// </summary>
public interface IInMemoryConfigurationBuilder
{
    /// <summary>
    /// Configures the serializer to use for message serialization and deserialization.
    /// </summary>
    /// <param name="serializer">The string serializer to use for messages.</param>
    /// <returns>The configuration builder instance for method chaining.</returns>
    IInMemoryConfigurationBuilder WithSerializer(ISerializer<string> serializer);
}
