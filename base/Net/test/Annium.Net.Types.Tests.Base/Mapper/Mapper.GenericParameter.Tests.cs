using System.Collections.Generic;
using Annium.Net.Types.Refs;
using Annium.Testing;
using Namotion.Reflection;

namespace Annium.Net.Types.Tests.Base.Mapper;

public abstract class MapperGenericParameterTestsBase : TestBase
{
    protected MapperGenericParameterTestsBase(ITestProvider testProvider) : base(testProvider)
    {
    }

    public void GenericParameter_NotNullable_Base()
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

    public void GenericParameter_Nullable_Base()
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