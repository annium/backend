using Annium.Net.Types.Tests.Base.Mapper;
using Xunit;

namespace Annium.Net.Types.Tests.Mapper;

public class MapperEnumTests : MapperEnumTestsBase
{
    public MapperEnumTests() : base(new TestProvider())
    {
    }

    [Fact]
    public void Enum()
    {
        Enum_Base();
    }
}