using Annium.Net.Types.Refs;
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
        var modelRef = Map(target).As<BaseTypeRef>();

        // assert
        modelRef.Name.Is(Refs.BaseType.Int);
        Models.IsEmpty();
    }
}