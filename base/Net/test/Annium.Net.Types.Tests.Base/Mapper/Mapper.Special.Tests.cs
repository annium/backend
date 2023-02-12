using System.Threading.Tasks;
using Annium.Net.Types.Refs;
using Annium.Testing;
using Namotion.Reflection;

namespace Annium.Net.Types.Tests.Base.Mapper;

public abstract class MapperSpecialTestsBase : TestBase
{
    protected MapperSpecialTestsBase(ITestProvider testProvider) : base(testProvider)
    {
    }

    public void Task_Generic_Nullable_Base()
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

    public void Task_Generic_NotNullable_Base()
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

    public void Task_NonGeneric_Base()
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