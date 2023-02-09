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
        var modelRef = Map(target);

        // assert
        modelRef.HasNamespace.IsFalse();
        modelRef.Name.Is(Net.Types.Models.BaseType.Int);
        Models.IsEmpty();
    }
}