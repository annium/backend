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
        mapper.Map<string>(Sex.Male).IsEqual("Male");
        mapper.Map<Sex>("female").IsEqual(Sex.Female);
    }

    private IMapper GetMapper() => new ServiceContainer()
        .AddRuntimeTools(Assembly.GetCallingAssembly(), false)
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