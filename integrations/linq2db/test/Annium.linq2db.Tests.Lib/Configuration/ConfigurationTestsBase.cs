using Annium.linq2db.Extensions.Configuration.Extensions;
using Annium.linq2db.Tests.Lib.Db;
using Annium.Testing;
using Annium.Testing.Lib;

namespace Annium.linq2db.Tests.Lib.Configuration;

public class ConfigurationTestsBase : TestBase
{
    protected ConfigurationTestsBase()
    {
        AddServicePack<LibServicePack>();
    }

    protected void Metadata_IsValid_Base()
    {
        // arrange
        using var conn = Get<Connection>();
        var databaseMetadata = conn.MappingSchema.Describe();

        // assert
        databaseMetadata.Tables.Has(3);
    }
}