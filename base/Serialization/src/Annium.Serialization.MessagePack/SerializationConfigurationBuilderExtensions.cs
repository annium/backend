using System;
using Annium.Serialization.Abstractions;
using Annium.Serialization.MessagePack.Internal;

namespace Annium.Serialization.MessagePack;

public static class SerializationConfigurationBuilderExtensions
{
    public static ISerializationConfigurationBuilder WithMessagePack(
        this ISerializationConfigurationBuilder builder,
        bool isDefault = false
    ) => builder
        .Register<ReadOnlyMemory<byte>, MessagePackSerializer>(Constants.MediaType, isDefault);
}