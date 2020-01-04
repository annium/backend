using Annium.Testing;

namespace Annium.Serialization.BinaryString.Tests
{
    public class HexStringSerializerTests
    {
        [Fact]
        public void Serialization_Deserialization_Works()
        {
            // arrange
            var data = new byte[] { 25, 17, 89, 36, 15 };

            // act
            var serialized = HexStringSerializer.Instance.Serialize(data);
            var deserialized = HexStringSerializer.Instance.Deserialize(serialized);

            // assert
            deserialized.IsEqual(data);
        }
    }
}