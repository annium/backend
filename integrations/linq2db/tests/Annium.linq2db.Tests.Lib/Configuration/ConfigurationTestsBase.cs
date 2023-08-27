using Annium.linq2db.Extensions.Configuration.Extensions;
using Annium.linq2db.Tests.Lib.Db;
using Annium.Testing;
using Annium.Testing.Lib;
using Xunit.Abstractions;

namespace Annium.linq2db.Tests.Lib.Configuration;

public class ConfigurationTestsBase : TestBase
{
    protected ConfigurationTestsBase(ITestOutputHelper outputHelper) : base(outputHelper)
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