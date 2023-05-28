using Annium.linq2db.Tests.Lib.Configuration;
using Xunit;

namespace Annium.linq2db.Testing.Sqlite.Tests.Configuration;

public class ConfigurationTests : ConfigurationTestsBase
{
    public ConfigurationTests()
    {
        AddServicePack<ServicePack>();
    }

    [Fact]
    public void Metadata_IsValid()
    {
        Metadata_IsValid_Base();
    }
}