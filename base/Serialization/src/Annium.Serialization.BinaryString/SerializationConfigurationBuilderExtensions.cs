using Annium.Serialization.Abstractions;
using Annium.Serialization.BinaryString.Internal;
using Constants = Annium.Serialization.BinaryString.Constants;

namespace Annium.Core.DependencyInjection;

public static class SerializationConfigurationBuilderExtensions
{
    public static ISerializationConfigurationBuilder WithBinaryString(
        this ISerializationConfigurationBuilder builder,
        bool isDefault = false
    ) => builder
        .Register<byte[], string, HexStringSerializer>(Constants.MediaType, isDefault);
}