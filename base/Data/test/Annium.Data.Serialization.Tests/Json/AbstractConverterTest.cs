using System;
using System.Text.Json;
using Annium.Core.DependencyInjection;
using Annium.Testing;

namespace Annium.Data.Serialization.Tests.Json
{
    public class AbstractConverterTest
    {
        [Fact]
        public void Serialization_Plain_Works()
        {
            // arrange
            var settings = GetSettings();
            Base a = new ChildA { A = 1 };
            Base b = new ChildB { B = 2 };
            var arr = new[] { a, b };

            // act
            var result = JsonSerializer.Serialize(arr, settings);

            // assert
            result.IsEqual(@"[{""a"":1},{""b"":2}]");
        }

        [Fact]
        public void Serialization_Generic_Works()
        {
            // arrange
            var settings = GetSettings();
            Base a = new ChildA { A = 1 };
            Base b = new ChildB { B = 2 };
            BaseContainer<Base> container = new DataContainer<Base> { Data = new[] { a, b } };

            // act
            var result = JsonSerializer.Serialize(container, settings);

            // assert
            result.IsEqual(@"{""data"":[{""a"":1},{""b"":2}]}");
        }

        [Fact]
        public void Deserialization_Plain_Works()
        {
            // arrange
            var settings = GetSettings();
            Base a = new ChildA { A = 1 };
            Base b = new ChildB { B = 2 };
            BaseContainer<Base> container = new DataContainer<Base> { Data = new[] { a, b } };
            var str = JsonSerializer.Serialize(container, settings);

            // act
            var result = JsonSerializer.Deserialize<BaseContainer<Base>>(str, settings);

            // assert
            result.As<DataContainer<Base>>().Data.Has(2);
            var data = ((DataContainer<Base>)result).Data;
            data.At(0).As<ChildA>().A.IsEqual(1);
            data.At(1).As<ChildB>().B.IsEqual(2);
        }

        [Fact]
        public void Deserialization_Generic_Works()
        {
            // arrange
            var settings = GetSettings();
            Base a = new ChildA { A = 1 };
            Base b = new ChildB { B = 2 };
            var arr = new[] { a, b };
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

    public abstract class BaseContainer<T> { }

    public class DataContainer<T> : BaseContainer<T>
    {
        public T[] Data { get; set; } = Array.Empty<T>();
    }

    public class DemoContainer<T> : BaseContainer<T>
    {
        public T[] Demo { get; set; } = Array.Empty<T>();
    }
}