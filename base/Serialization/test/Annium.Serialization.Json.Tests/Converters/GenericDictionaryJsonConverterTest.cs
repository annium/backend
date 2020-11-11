using System;
using System.Collections.Generic;
using Annium.Core.DependencyInjection;
using Annium.Core.Runtime.Types;
using Annium.Serialization.Abstractions;
using Annium.Serialization.Json.Tests.Converters.GenericDictionaryConverter;
using Annium.Testing;
using Xunit;

namespace Annium.Serialization.Json.Tests.Converters
{
    public class GenericDictionaryJsonConverterTest
    {
        [Fact]
        public void Serialization_Basic_Works()
        {
            // arrange
            var serializer = GetSerializer();
            var key = Guid.NewGuid();
            var value = Guid.NewGuid();
            var source = new Dictionary<Guid, Guid> { { key, value } };

            // act
            var result = serializer.Serialize(source);

            // assert
            result.IsEqual($@"{{""{key}"":""{value}""}}");
        }

        [Fact]
        public void Serialization_ObjectKey_Works()
        {
            // arrange
            var serializer = GetSerializer();
            var key = new Key { X = 1, Y = 2 };
            var value = new Key { X = 3, Y = 4 };
            var source = new Dictionary<Key, Key> { { key, value } };

            // act
            var result = serializer.Serialize(source);

            // assert
            result.IsEqual(@"{""{\""x\"":1,\""y\"":2}"":{""x"":3,""y"":4}}");
        }

        [Fact]
        public void Deserialization_Basic_Works()
        {
            // arrange
            var serializer = GetSerializer();
            var key = Guid.NewGuid();
            var value = Guid.NewGuid();
            var source = new Dictionary<Guid, Guid> { { key, value } };
            var str = serializer.Serialize(source);

            // act
            var result = serializer.Deserialize<IReadOnlyDictionary<Guid, Guid>>(str);

            // assert
            result.At(key).IsEqual(value);
        }

        [Fact]
        public void Deserialization_ObjectKey_Works()
        {
            // arrange
            var serializer = GetSerializer();
            var key = new Key { X = 1, Y = 2 };
            var value = new Key { X = 3, Y = 4 };
            var source = new Dictionary<Key, Key> { { key, value } };
            var str = serializer.Serialize(source);

            // act
            var result = serializer.Deserialize<IReadOnlyDictionary<Key, Key>>(str);

            // assert
            result.At(key).IsEqual(value);
        }

        private ISerializer<string> GetSerializer() => StringSerializer.Configure(
            opts => opts.ConfigureDefault(TypeManager.GetInstance(GetType().Assembly, false))
        );
    }

    namespace GenericDictionaryConverter
    {
        public struct Key
        {
            public int X { get; set; }
            public int Y { get; set; }
        }
    }
}