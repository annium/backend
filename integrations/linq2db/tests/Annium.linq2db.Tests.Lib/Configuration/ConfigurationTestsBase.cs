using Annium.linq2db.Extensions.Configuration.Extensions;
using Annium.linq2db.Tests.Lib.Db;
using Annium.Testing;
using Annium.Testing.Collection;
using Xunit;

namespace Annium.linq2db.Tests.Lib.Configuration;

/// <summary>
/// Base class for testing linq2db configuration and database metadata validation
/// </summary>
public class ConfigurationTestsBase : TestBase
{
    /// <summary>
    /// Initializes a new instance of the ConfigurationTestsBase class
    /// </summary>
    /// <param name="outputHelper">Test output helper for logging</param>
    protected ConfigurationTestsBase(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        AddServicePack<LibServicePack>();
    }

    /// <summary>
    /// Base test method to validate that database metadata is correctly configured with expected number of tables
    /// </summary>
    protected void Metadata_IsValid_Base()
    {
        // arrange
        using var conn = Get<Connection>();
        var databaseMetadata = conn.MappingSchema.Describe();

        // assert
        databaseMetadata.Tables.Has(3);
    }
}
