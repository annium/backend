using System.Collections.Generic;
using Annium.Net.Types.Refs;
using Annium.Testing;
using Namotion.Reflection;
using Xunit;

namespace Annium.Net.Types.Tests.Mapper;

public class MapperArrayTests : TestBase
{
    [Fact]
    public void Array()
    {
        // arrange
        var target = typeof(int[]).ToContextualType();

        // act
        var modelRef = Map(target);

        // assert
        modelRef
            .As<ArrayRef>().Value
            .As<BaseTypeRef>().Name.Is(BaseType.Int);
        Models.IsEmpty();
    }

    [Fact]
    public void ArrayLike()
    {
        // arrange
        var target = typeof(HashSet<>).ToContextualType();

        // act
        var modelRef = Map(target);

        // assert
        modelRef
            .As<ArrayRef>().Value
            .As<GenericParameterRef>().Name.Is("T");
        Models.IsEmpty();
    }
}