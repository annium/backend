using Annium.Net.Types.Tests.Base.Mapper;
using Xunit;

namespace Annium.Net.Types.Serialization.Json.Tests.Mapper;

public class MapperIncludedTests : MapperIncludedTestsBase
{
    public MapperIncludedTests() : base(new TestProvider())
    {
    }

    [Fact]
    public void Included()
    {
        Included_Base();
    }
}