using Annium.Net.Types.Refs;
using Annium.Testing;
using Namotion.Reflection;

namespace Annium.Net.Types.Tests.Base.Mapper;

public abstract class MapperBaseTypeTestsBase : TestBase
{
    protected MapperBaseTypeTestsBase(ITestProvider testProvider) : base(testProvider)
    {
    }

    public void BaseType_Base()
    {
        // arrange
        var target = typeof(int).ToContextualType();

        // act
        var modelRef = Map(target).As<BaseTypeRef>();

        // assert
        modelRef.Name.Is(BaseType.Int);
        Models.IsEmpty();
    }
}