using Annium.Extensions.DependencyInjection;
using Annium.Testing;
using Newtonsoft.Json;

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
            var result = JsonConvert.SerializeObject(arr, settings);

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
            var str = JsonConvert.SerializeObject(arr, settings);

            // act
            var result = JsonConvert.DeserializeObject<Base[]>(str, settings);

            // assert
            result.Has(2);
            result.At(0).As<ChildA>().A.IsEqual(1);
            result.At(1).As<ChildB>().B.IsEqual(2);
        }

        private JsonSerializerSettings GetSettings() => new JsonSerializerSettings()
            .ConfigureAbstractConverter();

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
}