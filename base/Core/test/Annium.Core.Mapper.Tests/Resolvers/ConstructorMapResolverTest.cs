using System.Reflection;
using Annium.Core.DependencyInjection;
using Annium.Testing;
using Xunit;

namespace Annium.Core.Mapper.Tests.Resolvers;

public class ConstructorMapResolverTest
{
    [Fact]
    public void ConstructorMapping_Works()
    {
        // arrange
        var mapper = GetMapper();
        var first = new A { Name = "first" };
        var second = new A { Name = "second" };

        // act
        var one = mapper.Map<B>(first);
        var arr = mapper.Map<B[]>(new[] { first, second });

        // assert
        one.Name.IsEqual(first.Name);
        arr.Has(2);
        arr.At(0).Name.IsEqual(first.Name);
        arr.At(1).Name.IsEqual(second.Name);
    }


    private IMapper GetMapper() => new ServiceContainer()
        .AddRuntime(Assembly.GetCallingAssembly())
        .AddMapper(autoload: false)
        .BuildServiceProvider()
        .Resolve<IMapper>();

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
}