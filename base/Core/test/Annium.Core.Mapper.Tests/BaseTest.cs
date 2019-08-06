using Annium.Core.DependencyInjection;
using Annium.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.Mapper.Tests
{
    public class BaseTest
    {
        [Fact]
        public void SameType_ReturnsSource()
        {
            // arrange
            var mapper = GetMapper();
            var value = new A() { Name = "name" };

            // act
            var result = mapper.Map<A>(value);

            // assert
            result.IsEqual(value);
        }

        [Fact]
        public void ConstructorMapping_Works()
        {
            // arrange
            var mapper = GetMapper();
            var first = new A() { Name = "first" };
            var second = new A() { Name = "second" };

            // act
            var one = mapper.Map<B>(first);
            var arr = mapper.Map<B[]>(new [] { first, second });

            // assert
            one.Name.IsEqual(first.Name);
            arr.Has(2);
            arr.At(0).Name.IsEqual(first.Name);
            arr.At(1).Name.IsEqual(second.Name);
        }

        [Fact]
        public void PropertyMapping_Works()
        {
            // arrange
            var mapper = GetMapper();
            var value = new A() { Name = "name" };

            // act
            var result = mapper.Map<C>(value);

            // assert
            result.Name.IsEqual(value.Name);
        }

        [Fact]
        public void Nesting_Works()
        {
            // arrange
            var mapper = GetMapper();
            var value = new D(new A() { Name = "name" }, "nice");

            // act
            var result = mapper.Map<E>(value);

            // assert
            result.Inner.Name.IsEqual(value.Inner.Name);
            result.Value.IsEqual(value.Value);
        }

        [Fact]
        public void NullableNesting_Works()
        {
            // arrange
            var mapper = GetMapper();
            var value = new D(null, "nice");

            // act
            var result = mapper.Map<E>(value);

            // assert
            result.Inner.IsDefault();
            result.Value.IsEqual(value.Value);
        }

        private IMapper GetMapper() =>
            new ServiceCollection().AddMapper().BuildServiceProvider().GetRequiredService<IMapper>();

        private class A
        {
            public string Name { get; set; }
        }

        private class B
        {
            public string Name { get; }

            public B(string name)
            {
                Name = name;
            }
        }

        private class C
        {
            public string Name { get; set; }
        }

        private class D
        {
            public A Inner { get; }

            public string Value { get; }

            public D(A inner, string value)
            {
                Inner = inner;
                Value = value;
            }
        }

        private class E
        {
            public B Inner { get; set; }

            public string Value { get; set; }
        }
    }
}