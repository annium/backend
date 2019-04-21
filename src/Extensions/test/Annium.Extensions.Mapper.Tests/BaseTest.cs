using Annium.Testing;

namespace Annium.Extensions.Mapper.Tests
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

        private static IMapper GetMapper()
        {
            var builder = new MapBuilder(new MapperConfiguration(), TypeResolverAccessor.TypeResolver);

            return new Mapper(builder);
        }

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
    }
}