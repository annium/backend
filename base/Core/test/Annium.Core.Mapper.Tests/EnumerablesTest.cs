using System.Collections.Generic;
using Annium.Core.DependencyInjection;
using Annium.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.Mapper.Tests
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
            var one = mapper.Map<B[]>(value);
            var two = mapper.Map<C[]>(value);

            // assert
            one.Has(1);
            one.At(0).Name.IsEqual(value[0].Name);
            two.Has(1);
            two.At(0).Name.IsEqual(value[0].Name);
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

        [Fact]
        public void ToIEnumerable_Works()
        {
            // arrange
            var mapper = GetMapper();
            var value = new [] { new A() { Name = "name" } };

            // act
            var result = mapper.Map<IEnumerable<B>>(value);

            // assert
            result.Has(1);
            result.At(0).Name.IsEqual(value[0].Name);
        }

        [Fact]
        public void ToIDictionary_Works()
        {
            // arrange
            var mapper = GetMapper();
            var value = new Dictionary<string, A> { { "one", new A() { Name = "name" } } };

            // act
            var result = mapper.Map<IReadOnlyDictionary<string, B>>(value);

            // assert
            result.Has(1);
            result.At("one").Name.IsEqual(value["one"].Name);
        }

        private IMapper GetMapper() =>
            new ServiceCollection().AddMapper().BuildServiceProvider().GetRequiredService<IMapper>();

        private class A
        {
            public string? Name { get; set; }
        }

        private class B
        {
            public string? Name { get; }

            public B(string? name)
            {
                Name = name;
            }
        }

        private class C
        {
            public string? Name { get; set; }
        }
    }
}