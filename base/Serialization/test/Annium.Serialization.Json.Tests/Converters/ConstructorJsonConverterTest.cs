using System.Collections.Generic;
using Annium.Core.Mapper;
using Annium.Core.Primitives;
using Annium.Core.Runtime.Types;
using Annium.Testing;
using Xunit;

namespace Annium.Serialization.Json.Tests.Converters
{
    public class ConstructorJsonConverterTest : TestBase
    {
        [Fact]
        public void Serialization_Base_Works()
        {
            // arrange
            var serializer = GetSerializer();

            var x = new A(5);

            // act
            var result = serializer.Serialize(x);

            // assert
            result.IsEqual(@"{""value"":5,""isOdd"":false}");
        }

        [Fact]
        public void Serialization_Inner_Works()
        {
            // arrange
            var serializer = GetSerializer();

            var x = new B(new A(2), new A(3));

            // act
            var result = serializer.Serialize(x);

            // assert
            result.IsEqual(@"{""one"":{""value"":2,""isOdd"":true},""two"":{""value"":3,""isOdd"":false}}");
        }

        [Fact]
        public void Serialization_Collection_Works()
        {
            // arrange
            var serializer = GetSerializer();

            var x = new IX[] { new B(new A(2), new A(3)), new A(1) };

            // act
            var result = serializer.Serialize(x);

            // assert
            result.IsEqual(@"[{""one"":{""value"":2,""isOdd"":true},""two"":{""value"":3,""isOdd"":false}},{""value"":1,""isOdd"":false}]");
        }

        [Fact]
        public void Deserialization_Base_Works()
        {
            // arrange
            var serializer = GetSerializer();
            var x = new A(5);
            var str = serializer.Serialize(x);

            // act
            var result = serializer.Deserialize<IX>(str);

            // assert
            result.IsEqual(x);
        }

        [Fact]
        public void Deserialization_Inner_Works()
        {
            // arrange
            var serializer = GetSerializer();
            var x = new B(new A(2), new A(3));
            var str = serializer.Serialize(x);

            // act
            var result = serializer.Deserialize<IX>(str);

            // assert
            result.IsEqual(x);
        }

        [Fact]
        public void Deserialization_Narrow_Works()
        {
            // arrange
            var serializer = GetSerializer();
            var x = new A(5);
            var str = serializer.Serialize(x);

            // act
            var result = serializer.Deserialize<Narrow>(str);

            // assert
            result.IsEqual(x);
        }

        [Fact]
        public void Deserialization_Collection_Works()
        {
            // arrange
            var serializer = GetSerializer();
            var x = new IX[] { new B(new A(2), new A(3)), new A(1) };
            var str = serializer.Serialize(x);

            // act
            var result = serializer.Deserialize<IReadOnlyCollection<IX>>(str);

            // assert
            var mapper = Mapper.GetFor(GetType().Assembly, GetType().Assembly.ShortName(), typeof(IEnumerable<>).Assembly.ShortName());
            result.IsEqual(x, "not equal", mapper);
        }

        [Fact]
        public void Deserialization_Record_Works()
        {
            // arrange
            var serializer = GetSerializer();
            var x = new MessageBase[] { new ErrorMessage("error"), new InfoMessage("info") };
            var str = serializer.Serialize(x);

            // act
            var result = serializer.Deserialize<IReadOnlyCollection<MessageBase>>(str);

            // assert
            result.Has(2);
            result.At(0).As<ErrorMessage>().Message.IsEqual("error");
            result.At(1).As<InfoMessage>().Message.IsEqual("info");
        }

        public class A : IX
        {
            public int Value { get; }
            public bool IsOdd => Value % 2 == 0;

            public A(int value)
            {
                Value = value;
            }
        }

        public class B : IX
        {
            public A One { get; }
            public A Two { get; }

            public B(A one, A two)
            {
                One = one;
                Two = two;
            }
        }

        public class Narrow
        {
            public int Value { get; }

            public Narrow(int value)
            {
                Value = value;
            }
        }

        public interface IX
        {
        }

        public record InfoMessage(string Message) : MessageBase;

        public record ErrorMessage(string Message) : MessageBase;

        public abstract record MessageBase
        {
            [ResolutionId]
            public string Tid => GetType().GetIdString();
        }
    }
}