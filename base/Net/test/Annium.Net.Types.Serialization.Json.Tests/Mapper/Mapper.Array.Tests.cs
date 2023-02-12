using Annium.Net.Types.Tests.Base.Mapper;
using Xunit;

namespace Annium.Net.Types.Serialization.Json.Tests.Mapper;

public class MapperArrayTests : MapperArrayTestsBase
{
    public MapperArrayTests() : base(new TestProvider())
    {
    }

    [Fact]
    public void Array()
    {
        Array_Base();
    }

    [Fact]
    public void ArrayLike()
    {
        ArrayLike_Base();
    }
}