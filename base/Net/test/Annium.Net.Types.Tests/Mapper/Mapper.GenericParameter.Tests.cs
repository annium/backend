using System.Collections.Generic;
using Annium.Net.Types.Refs;
using Annium.Testing;
using Namotion.Reflection;
using Xunit;

namespace Annium.Net.Types.Tests.Mapper;

public class MapperGenericParameterTests : TestBase
{
    [Fact]
    public void GenericParameter_NotNullable()
    {
        // arrange
        var target = typeof(List<>).GetGenericArguments()[0].ToContextualType();

        // act
        var modelRef = Map(target);

        // assert
        modelRef
            .As<GenericParameterRef>().Name.Is(target.Type.Name);
        Models.IsEmpty();
    }

    [Fact]
    public void GenericParameter_Nullable()
    {
        // arrange
        var target = typeof(Sample<>).ToContextualType().GetProperty(nameof(Sample<string>.Value))!.AccessorType;

        // act
        var modelRef = Map(target);

        // assert
        modelRef
            .As<NullableRef>().Value
            .As<GenericParameterRef>().Name.Is(target.Type.Name);
        Models.IsEmpty();
    }
}

file record Sample<T>(T? Value) where T : notnull;