using System.Reflection;
using Annium.Core.DependencyInjection;
using Annium.Core.Mapper.Attributes;
using Annium.Testing;
using Xunit;

namespace Annium.Core.Mapper.Tests.Resolvers;

public class EnumTest
{
    [Fact]
    public void EnumMapping_Works()
    {
        // arrange
        var mapper = GetMapper();

        // assert
        mapper.Map<string>(Sex.Male).Is("Male");
        mapper.Map<Sex>("female").Is(Sex.Female);
    }

    private IMapper GetMapper() => new ServiceContainer()
        .AddRuntime(Assembly.GetCallingAssembly())
        .AddMapper(autoload: false)
        .BuildServiceProvider()
        .Resolve<IMapper>();

    [AutoMapped]
    private enum Sex
    {
        Male,
        Female
    }
}