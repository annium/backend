using Annium.Net.Types.Refs;
using Annium.Testing;
using Namotion.Reflection;
using Xunit;

namespace Annium.Net.Types.Tests.Mapper;

public class MapperNullableTests : TestBase
{
    [Fact]
    public void Nullable_BaseType_Struct()
    {
        // arrange
        var target = typeof(int?).ToContextualType();

        // act
        var model = Map(target);

        // assert
        model
            .As<NullableRef>().Value
            .As<BaseTypeRef>().Name.Is(BaseType.Int);
        Models.IsEmpty();
    }

    [Fact]
    public void Nullable_BaseType_Class()
    {
        // arrange
        var target = typeof(Sample).ToContextualType().GetProperty(nameof(Sample.Value))!.AccessorType;

        // act
        var model = Map(target);

        // assert
        model
            .As<NullableRef>().Value
            .As<BaseTypeRef>().Name.Is(BaseType.String);
        Models.IsEmpty();
    }
}

file record Sample(string? Value);