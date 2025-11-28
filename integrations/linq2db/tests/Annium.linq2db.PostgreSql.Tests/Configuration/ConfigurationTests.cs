using Annium.linq2db.Tests.Lib.Configuration;
using Xunit;

namespace Annium.linq2db.PostgreSql.Tests.Configuration;

/// <summary>
/// Tests for PostgreSQL-specific linq2db configuration and metadata validation
/// </summary>
public class ConfigurationTests : ConfigurationTestsBase
{
    /// <summary>
    /// Initializes a new instance of the ConfigurationTests class
    /// </summary>
    /// <param name="outputHelper">Test output helper for logging</param>
    public ConfigurationTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        RegisterServicePack<ServicePack>();
    }

    /// <summary>
    /// Tests that PostgreSQL database metadata is valid and properly configured
    /// </summary>
    [Fact]
    public void Metadata_IsValid()
    {
        Metadata_IsValid_Base();
    }
}
