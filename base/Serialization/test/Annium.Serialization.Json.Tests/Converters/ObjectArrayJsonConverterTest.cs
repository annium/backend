using Annium.Serialization.Json.Attributes;
using Annium.Testing;
using Xunit;

namespace Annium.Serialization.Json.Tests.Converters;

public class ObjectArrayJsonConverterTest : TestBase
{
    [Fact]
    public void Serialization_Base_Works()
    {
        // arrange
        var serializer = GetSerializer();

        var x = new A { Value = 5, IsFinal = 3 };
        x.SetData("demo");

        // act
        var result = serializer.Serialize(x);

        // assert
        result.Is(@"[""demo"",3,5]");
    }

    [Fact]
    public void Deserialization_Base_Works()
    {
        // arrange
        var serializer = GetSerializer();
        var x = new A { Value = 5, IsFinal = 3 };
        x.SetData("demo");
        var str = serializer.Serialize(x);

        // act
        var result = serializer.Deserialize<A>(str);

        // assert
        result.Is(x);
    }

    [JsonAsArray]
    public record A
    {
        public int Value { get; set; }
        public string Data { get; private set; } = string.Empty;
        public byte IsFinal;
        public bool IsOdd => Value % 2 == 0;
        public void SetData(string data) => Data = data;
    }
}