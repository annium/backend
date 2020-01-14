using System;
using System.Text.Json;
using Annium.Core.Reflection;
using Annium.Serialization.Abstractions;
using Annium.Testing;
using Xunit;

namespace Annium.Serialization.Json.Tests
{
    public class AbstractConverterTest
    {
        [Fact]
        public void Serialization_SignaturePlain_Works()
        {
            // arrange
            var serializer = GetSerializer();
            Base a = new ChildA { A = 1 };
            Base b = new ChildB { B = 2 };
            var arr = new[] { a, b };

            // act
            var result = serializer.Serialize(arr);

            // assert
            result.IsEqual(@"[{""a"":1},{""b"":2}]");
        }

        [Fact]
        public void Serialization_KeyPlain_Works()
        {
            // arrange
            var serializer = GetSerializer();
            KeyBase a = new KeyChildA { Value = 1 };
            KeyBase b = new KeyChildB { Value = 2 };
            var arr = new[] { a, b };

            // act
            var result = serializer.Serialize(arr);

            // assert
            result.IsEqual(@"[{""value"":1,""type"":""a""},{""value"":2,""type"":""b""}]");
        }

        [Fact]
        public void Serialization_SignatureGeneric_Works()
        {
            // arrange
            var serializer = GetSerializer();
            Base a = new ChildA { A = 1 };
            Base b = new ChildB { B = 2 };
            BaseContainer<Base> container = new DataContainer<Base> { Data = new[] { a, b } };

            // act
            var result = serializer.Serialize(container);

            // assert
            result.IsEqual(@"{""data"":[{""a"":1},{""b"":2}]}");
        }

        [Fact]
        public void Serialization_KeyGeneric_Works()
        {
            // arrange
            var serializer = GetSerializer();
            KeyBase a = new KeyChildA { Value = 1 };
            KeyBase b = new KeyChildB { Value = 2 };
            KeyBaseContainer<KeyBase> container = new KeyDataContainer<KeyBase> { Items = new[] { a, b } };

            // act
            var result = serializer.Serialize(container);

            // assert
            result.IsEqual(@"{""items"":[{""value"":1,""type"":""a""},{""value"":2,""type"":""b""}],""type"":""a""}");
        }

        [Fact]
        public void Deserialization_SignaturePlain_Works()
        {
            // arrange
            var serializer = GetSerializer();
            Base a = new ChildA { A = 1 };
            Base b = new ChildB { B = 2 };
            var arr = new[] { a, b };
            var str = serializer.Serialize(arr);

            // act
            var result = serializer.Deserialize<Base[]>(str);

            // assert
            result.Has(2);
            result.At(0).As<ChildA>().A.IsEqual(1);
            result.At(1).As<ChildB>().B.IsEqual(2);
        }

        [Fact]
        public void Deserialization_KeyPlain_Works()
        {
            // arrange
            var serializer = GetSerializer();
            KeyBase a = new KeyChildA { Value = 1 };
            KeyBase b = new KeyChildB { Value = 2 };
            var arr = new[] { a, b };
            var str = serializer.Serialize(arr);

            // act
            var result = serializer.Deserialize<KeyBase[]>(str);

            // assert
            result.Has(2);
            result.At(0).As<KeyChildA>().Value.IsEqual(1);
            result.At(1).As<KeyChildB>().Value.IsEqual(2);
        }

        [Fact]
        public void Deserialization_SignatureGeneric_Works()
        {
            // arrange
            var serializer = GetSerializer();
            Base a = new ChildA { A = 1 };
            Base b = new ChildB { B = 2 };
            BaseContainer<Base> container = new DataContainer<Base> { Data = new[] { a, b } };
            var str = serializer.Serialize(container);

            // act
            var result = serializer.Deserialize<BaseContainer<Base>>(str);

            // assert
            var data = result.As<DataContainer<Base>>().Data;
            data.Has(2);
            data.At(0).As<ChildA>().A.IsEqual(1);
            data.At(1).As<ChildB>().B.IsEqual(2);
        }

        [Fact]
        public void Deserialization_KeyGeneric_Works()
        {
            // arrange
            var serializer = GetSerializer();
            KeyBase a = new KeyChildA { Value = 1 };
            KeyBase b = new KeyChildB { Value = 2 };
            KeyBaseContainer<KeyBase> container = new KeyDataContainer<KeyBase> { Items = new[] { a, b } };
            var str = serializer.Serialize(container);

            // act
            var result = serializer.Deserialize<KeyBaseContainer<KeyBase>>(str);

            // assert
            var data = result.As<KeyDataContainer<KeyBase>>().Items;
            data.Has(2);
            data.At(0).As<KeyChildA>().Value.IsEqual(1);
            data.At(1).As<KeyChildB>().Value.IsEqual(2);
        }

        private ISerializer<string> GetSerializer() => StringSerializer.Default;
    }

    public abstract class Base
    {
    }

    public class ChildA : Base
    {
        public int A { get; set; }
    }

    public class ChildB : Base
    {
        public int B { get; set; }
    }

    public abstract class KeyBase
    {
        [ResolveField] public char Type { get; set; }
    }

    [ResolveKey("a")]
    public class KeyChildA : KeyBase
    {
        public int Value { get; set; }

        public KeyChildA()
        {
            Type = 'a';
        }
    }

    [ResolveKey("b")]
    public class KeyChildB : KeyBase
    {
        public int Value { get; set; }

        public KeyChildB()
        {
            Type = 'b';
        }
    }

    public abstract class BaseContainer<T>
    {
    }

    public class DataContainer<T> : BaseContainer<T>
    {
        public T[] Data { get; set; } = Array.Empty<T>();
    }

    public class DemoContainer<T> : BaseContainer<T>
    {
        public T[] Demo { get; set; } = Array.Empty<T>();
    }

    public abstract class KeyBaseContainer<T>
    {
        [ResolveField] public char Type { get; set; }
    }

    [ResolveKey("a")]
    public class KeyDataContainer<T> : KeyBaseContainer<T>
    {
        public T[] Items { get; set; } = Array.Empty<T>();

        public KeyDataContainer()
        {
            Type = 'a';
        }
    }

    [ResolveKey("b")]
    public class KeyDemoContainer<T> : KeyBaseContainer<T>
    {
        public T[] Demo { get; set; } = Array.Empty<T>();

        public KeyDemoContainer()
        {
            Type = 'b';
        }
    }
}