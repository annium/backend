using Annium.Serialization.Abstractions;
using Annium.Serialization.BinaryString.Internal;

namespace Annium.Serialization.BinaryString;

public static class SerializationConfigurationBuilderExtensions
{
    public static ISerializationConfigurationBuilder WithBinaryString(
        this ISerializationConfigurationBuilder builder,
        bool isDefault = false
    ) => builder
        .Register<byte[], string, HexStringSerializer>(Constants.MediaType, isDefault);
}