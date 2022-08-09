using Annium.linq2db.Extensions.Configuration.Extensions;
using Annium.linq2db.Extensions.Tests.Db;
using Annium.Testing;
using Annium.Testing.Lib;
using Xunit;

namespace Annium.linq2db.Extensions.Tests.Configuration;

public class ConfigurationTests : TestBase
{
    public ConfigurationTests()
    {
        AddServicePack<ServicePack>();
    }

    [Fact]
    public void Metadata_IsValid()
    {
        // arrange
        using var conn = Get<Connection>();
        var databaseMetadata = conn.MappingSchema.Describe();

        // assert
        databaseMetadata.Tables.Has(2);
    }
}