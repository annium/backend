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
            var value = new A() { Name = "name" };

            // act
            var result = mapper.Map<B>(value);

            // assert
            result.Name.IsEqual(value.Name);
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

        private IMapper GetMapper()
        {
            var builder = new MapBuilder(new MapperConfiguration());

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