using Annium.Net.Types.Tests.Base.Mapper;
using Xunit;

namespace Annium.Net.Types.Tests.Mapper;

public class MapperBaseTypeTests : MapperBaseTypeTestsBase
{
    public MapperBaseTypeTests() : base(new TestProvider())
    {
    }

    [Fact]
    public void BaseType()
    {
        BaseType_Base();
    }
}