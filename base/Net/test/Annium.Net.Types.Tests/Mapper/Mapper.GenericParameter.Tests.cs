using System.Collections.Generic;
using Annium.Testing;
using Namotion.Reflection;
using Xunit;

namespace Annium.Net.Types.Tests.Mapper;

public class MapperGenericParameterTests : TestBase
{
    [Fact]
    public void GenericParameter()
    {
        // arrange
        var target = typeof(List<>).GetGenericArguments()[0].ToContextualType();

        // act
        var modelRef = Map(target);

        // assert
        modelRef.HasNamespace.IsFalse();
        modelRef.Name.Is(target.Type.Name);
    }
}