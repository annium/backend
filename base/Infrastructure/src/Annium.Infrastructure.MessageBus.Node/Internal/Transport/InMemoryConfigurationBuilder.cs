using System;
using Annium.Serialization.Abstractions;

namespace Annium.Infrastructure.MessageBus.Node.Internal.Transport;

/// <summary>
/// Builder implementation for configuring in-memory MessageBus transport.
/// </summary>
internal class InMemoryConfigurationBuilder : IInMemoryConfigurationBuilder
{
    /// <summary>
    /// The serializer to use for message serialization.
    /// </summary>
    private ISerializer<string>? _serializer;

    /// <summary>
    /// Configures the serializer to use for message serialization and deserialization.
    /// </summary>
    /// <param name="serializer">The string serializer to use for messages.</param>
    /// <returns>The configuration builder instance for method chaining.</returns>
    public IInMemoryConfigurationBuilder WithSerializer(ISerializer<string> serializer)
    {
        _serializer = serializer;

        return this;
    }

    /// <summary>
    /// Builds the in-memory configuration with the specified settings.
    /// </summary>
    /// <returns>The configured InMemoryConfiguration instance.</returns>
    public InMemoryConfiguration Build()
    {
        if (_serializer is null)
            throw new ArgumentException("MessageBus serializer is not configured");

        return new InMemoryConfiguration(_serializer);
    }
}
