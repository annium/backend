using System.Collections.Generic;
using Annium.Testing;

namespace Annium.Extensions.Mapper.Tests
{
    public class EnumerablesTest
    {
        [Fact]
        public void ToArray_Works()
        {
            // arrange
            var mapper = GetMapper();
            var value = new [] { new A() { Name = "name" } };

            // act
            var result = mapper.Map<B[]>(value);

            // assert
            result.Has(1);
            result.At(0).Name.IsEqual(value[0].Name);
        }

        [Fact]
        public void ToCollection_Works()
        {
            // arrange
            var mapper = GetMapper();
            var value = new [] { new A() { Name = "name" } };

            // act
            var result = mapper.Map<List<B>>(value);

            // assert
            result.Has(1);
            result.At(0).Name.IsEqual(value[0].Name);
        }

        [Fact]
        public void ToDictionary_Works()
        {
            // arrange
            var mapper = GetMapper();
            var value = new Dictionary<string, A> { { "one", new A() { Name = "name" } } };

            // act
            IDictionary<string, B> result = mapper.Map<Dictionary<string, B>>(value);

            // assert
            result.Has(1);
            result.At("one").Name.IsEqual(value["one"].Name);
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
    }
}