using System.Threading.Tasks;
using Annium.Net.Types.Refs;
using Annium.Testing;
using Namotion.Reflection;
using Xunit;

namespace Annium.Net.Types.Tests.Mapper;

public class MapperSpecialTests : TestBase
{
    [Fact]
    public void Task_Generic_Nullable()
    {
        // arrange
        var target = typeof(Sample).ToContextualType().GetProperty(nameof(Sample.NullableTask))!.AccessorType;

        // act
        var modelRef = Map(target);

        // assert
        modelRef
            .As<PromiseRef>().Value
            .As<NullableRef>().Value
            .As<BaseTypeRef>().Name.Is(BaseType.String);
        Models.IsEmpty();
    }

    [Fact]
    public void Task_Generic_NotNullable()
    {
        // arrange
        var target = typeof(Sample).ToContextualType().GetProperty(nameof(Sample.NotNullableTask))!.AccessorType;

        // act
        var modelRef = Map(target);

        // assert
        modelRef
            .As<PromiseRef>().Value
            .As<BaseTypeRef>().Name.Is(BaseType.String);
        Models.IsEmpty();
    }

    [Fact]
    public void Task_NonGeneric()
    {
        // arrange
        var target = typeof(Task).ToContextualType();

        // act
        var modelRef = Map(target);

        // assert
        modelRef
            .As<PromiseRef>().Value
            .IsDefault();
        Models.IsEmpty();
    }
}

file record Sample(Task<string?> NullableTask, Task<string> NotNullableTask);