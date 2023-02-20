using Annium.Net.Types.Refs;
using Annium.Testing;
using Namotion.Reflection;

namespace Annium.Net.Types.Tests.Base.Mapper;

public abstract class MapperExcludedTestsBase : TestBase
{
    protected MapperExcludedTestsBase(ITestProvider testProvider) : base(testProvider)
    {
    }

    public void Excluded_Base()
    {
        // arrange
        Config.Exclude(Match.Is(typeof(Sample)));
        var target = typeof(Sample).ToContextualType();

        // act
        var modelRef = Map(target).As<StructRef>();

        // assert
        modelRef.Namespace.Is(typeof(Sample).Namespace);
        modelRef.Name.Is(nameof(Sample));
        modelRef.Args.IsEmpty();
        Models.IsEmpty();
    }
}

file record Sample;