using Annium.Extensions.Primitives;
using Annium.Serialization.Abstractions;

namespace Annium.Serialization.BinaryString
{
    public static class HexStringSerializer
    {
        public static ISerializer<byte[], string> Instance { get; } = Serializer.Create<byte[], string>(
            value => value.ToHexString(),
            value => value.FromHexStringToByteArray()
        );
    }
}