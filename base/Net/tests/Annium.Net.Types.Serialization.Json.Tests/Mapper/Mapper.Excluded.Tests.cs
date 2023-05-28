using Annium.Net.Types.Tests.Base.Mapper;
using Xunit;

namespace Annium.Net.Types.Serialization.Json.Tests.Mapper;

public class MapperExcludedTests : MapperExcludedTestsBase
{
    public MapperExcludedTests() : base(new TestProvider())
    {
    }

    [Fact]
    public void Excluded()
    {
        Excluded_Base();
    }
}