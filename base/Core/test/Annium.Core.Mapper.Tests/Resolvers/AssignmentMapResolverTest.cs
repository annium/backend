using System.Reflection;
using Annium.Core.DependencyInjection;
using Annium.Testing;
using Xunit;

namespace Annium.Core.Mapper.Tests.Resolvers;

public class AssignmentMapResolverTest
{
    [Fact]
    public void AssignmentMapping_Works()
    {
        // arrange
        var mapper = GetMapper();
        var value = new A { Name = "name" };

        // act
        var result = mapper.Map<B>(value);

        // assert
        result.Name.IsEqual(value.Name);
    }

    [Fact]
    public void AssignmentMapping_WithExcessProperties_Works()
    {
        // arrange
        var mapper = GetMapper();
        var value = new A { Name = "name" };

        // act
        var result = mapper.Map<C>(value);

        // assert
        result.IsNotDefault();
    }

    private IMapper GetMapper() => new ServiceContainer()
        .AddRuntimeTools(Assembly.GetCallingAssembly(), false)
        .AddMapper(autoload: false)
        .BuildServiceProvider()
        .Resolve<IMapper>();

    private class A
    {
        public string? Name { get; set; }
    }

    private class B
    {
        public string? Name { get; set; }
    }

    private class C
    {
    }
}