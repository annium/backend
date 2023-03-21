using System.Reflection;
using Annium.Core.DependencyInjection;
using Annium.Testing;
using Xunit;

namespace Annium.Core.Mapper.Tests.Resolvers;

public class InstanceOfMapResolverTest
{
    [Fact]
    public void InstanceOf_Works()
    {
        // arrange
        var mapper = GetMapper();
        var value = new Payload();

        // act
        var result = mapper.Map<Payload>(value);

        // assert
        result.Is(value);
    }

    private IMapper GetMapper() => new ServiceContainer()
        .AddRuntime(Assembly.GetCallingAssembly())
        .AddMapper(autoload: false)
        .BuildServiceProvider()
        .Resolve<IMapper>();

    private class Payload
    {
    }
}