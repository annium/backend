using System.Collections.Generic;
using Annium.Net.Types.Refs;
using Annium.Testing;
using Namotion.Reflection;

namespace Annium.Net.Types.Tests.Base.Mapper;

public abstract class MapperArrayTestsBase : TestBase
{
    protected MapperArrayTestsBase(ITestProvider testProvider) : base(testProvider)
    {
    }

    public void Array_Base()
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

    public void ArrayLike_Base()
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