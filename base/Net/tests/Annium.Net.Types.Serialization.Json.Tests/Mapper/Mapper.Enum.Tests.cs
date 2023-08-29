using Annium.Net.Types.Tests.Base.Mapper;
using Xunit;
using Xunit.Abstractions;

namespace Annium.Net.Types.Serialization.Json.Tests.Mapper;

public class MapperEnumTests : MapperEnumTestsBase
{
    public MapperEnumTests(
        ITestOutputHelper outputHelper
    ) : base(
        new TestProvider(),
        outputHelper
    )
    {
    }

    [Fact]
    public void Enum()
    {
        Enum_Base();
    }
}