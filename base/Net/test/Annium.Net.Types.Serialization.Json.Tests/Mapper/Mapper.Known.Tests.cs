using Annium.Net.Types.Tests.Base.Mapper;
using Xunit;

namespace Annium.Net.Types.Serialization.Json.Tests.Mapper;

public class MapperKnownTests : MapperKnownTestsBase
{
    public MapperKnownTests() : base(new TestProvider())
    {
    }

    [Fact]
    public void Known()
    {
        Known_Base();
    }
}