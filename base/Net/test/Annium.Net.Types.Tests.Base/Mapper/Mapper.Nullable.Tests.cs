using Annium.Net.Types.Refs;
using Annium.Testing;
using Namotion.Reflection;

namespace Annium.Net.Types.Tests.Base.Mapper;

public abstract class MapperNullableTestsBase : TestBase
{
    protected MapperNullableTestsBase(ITestProvider testProvider) : base(testProvider)
    {
    }

    public void Nullable_BaseType_Struct_Base()
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

    public void Nullable_BaseType_Class_Base()
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