using Annium.linq2db.Tests.Lib.Configuration;
using Xunit;

namespace Annium.linq2db.PostgreSql.Tests.Configuration;

public class ConfigurationTests : ConfigurationTestsBase
{
    public ConfigurationTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        AddServicePack<ServicePack>();
    }

    [Fact]
    public void Metadata_IsValid()
    {
        Metadata_IsValid_Base();
    }
}
