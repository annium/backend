using Annium.Net.Types.Tests.Base.Mapper;
using Xunit;

namespace Annium.Net.Types.Serialization.Json.Tests.Mapper;

public class MapperGenericParameterTests : MapperGenericParameterTestsBase
{
    public MapperGenericParameterTests() : base(new TestProvider())
    {
    }

    [Fact]
    public void GenericParameter_NotNullable()
    {
        GenericParameter_NotNullable_Base();
    }

    [Fact]
    public void GenericParameter_Nullable()
    {
        GenericParameter_Nullable_Base();
    }
}