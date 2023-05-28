using Annium.Net.Types.Tests.Base.Mapper;
using Xunit;

namespace Annium.Net.Types.Serialization.Json.Tests.Mapper;

public class MapperSpecialTests : MapperSpecialTestsBase
{
    public MapperSpecialTests() : base(new TestProvider())
    {
    }

    [Fact]
    public void Task_Generic_Nullable()
    {
        Task_Generic_Nullable_Base();
    }

    [Fact]
    public void Task_Generic_NotNullable()
    {
        Task_Generic_NotNullable_Base();
    }

    [Fact]
    public void Task_NonGeneric()
    {
        Task_NonGeneric_Base();
    }
}