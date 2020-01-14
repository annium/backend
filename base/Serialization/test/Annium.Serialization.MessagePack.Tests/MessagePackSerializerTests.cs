using Annium.Testing;
using MessagePack;
using Xunit;

namespace Annium.Serialization.MessagePack.Tests
{
    public class MessagePackSerializerTests
    {
        [Fact]
        public void Serialization_Deserialization_Works()
        {
            // arrange
            var data = new Person { FirstName = "Max", LastName = "Madness" };

            // act
            var serialized = MessagePackSerializer.Instance.Serialize(data);
            var deserialized = MessagePackSerializer.Instance.Deserialize<Person>(serialized);

            // assert
            deserialized.IsNotDefault();
            deserialized.FirstName.IsEqual(data.FirstName);
            deserialized.LastName.IsEqual(data.LastName);
        }

        [MessagePackObject]
        public class Person
        {
            [Key(0)]
            public string FirstName { get; set; } = string.Empty;

            [Key(1)]
            public string LastName { get; set; } = string.Empty;
        }
    }
}