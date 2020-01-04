using Annium.Extensions.Primitives;
using Annium.Serialization.Abstractions;

namespace Annium.Serialization.BinaryString
{
    public class HexStringSerializer : ISerializer<byte[], string>
    {
        public static ISerializer<byte[], string> Instance { get; } = new HexStringSerializer();

        private HexStringSerializer()
        {
        }

        public byte[] Deserialize(string value)
        {
            return value.FromHexStringToByteArray();
        }

        public string Serialize(byte[] value)
        {
            return value.ToHexString();
        }
    }
}