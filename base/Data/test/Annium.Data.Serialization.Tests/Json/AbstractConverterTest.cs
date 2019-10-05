using System.Text.Json;
using Annium.Core.DependencyInjection;
using Annium.Testing;

namespace Annium.Data.Serialization.Tests.Json
{
    public class AbstractConverterTest
    {
        [Fact]
        public void Serialization_Works()
        {
            // arrange
            var settings = GetSettings();
            Base a = new ChildA { A = 1 };
            Base b = new ChildB { B = 2 };
            var arr = new [] { a, b };

            // act
            var result = JsonSerializer.Serialize(arr, settings);

            // assert
            result.IsEqual(@"[{""A"":1},{""B"":2}]");
        }

        [Fact]
        public void Deserialization_Works()
        {
            // arrange
            var settings = GetSettings();
            Base a = new ChildA { A = 1 };
            Base b = new ChildB { B = 2 };
            var arr = new [] { a, b };
            var str = JsonSerializer.Serialize(arr, settings);

            // act
            var result = JsonSerializer.Deserialize<Base[]>(str, settings);

            // assert
            result.Has(2);
            result.At(0).As<ChildA>().A.IsEqual(1);
            result.At(1).As<ChildB>().B.IsEqual(2);
        }

        private JsonSerializerOptions GetSettings() => new JsonSerializerOptions()
            .ConfigureAbstractConverter();
    }

    public abstract class Base { }

    public class ChildA : Base
    {
        public int A { get; set; }
    }

    public class ChildB : Base
    {
        public int B { get; set; }
    }
}