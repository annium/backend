using Annium.Net.Types.Tests.Base.Mapper;
using Xunit;

namespace Annium.Net.Types.Tests.Mapper;

public class MapperRecordTests : MapperRecordTestsBase
{
    public MapperRecordTests() : base(new TestProvider())
    {
    }

    [Fact]
    public void Interface()
    {
        Interface_Base();
    }

    [Fact]
    public void Implementation()
    {
        Implementation_Base();
    }
}