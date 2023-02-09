using Annium.Testing;
using Namotion.Reflection;
using Xunit;

namespace Annium.Net.Types.Tests.Mapper;

public class MapperBaseTypeTests : TestBase
{
    [Fact]
    public void BaseType()
    {
        // arrange
        var target = typeof(int).ToContextualType();

        // act
        var model = Map(target);

        // assert
        model.Name.Is("int");
        Models.IsEmpty();
    }
}