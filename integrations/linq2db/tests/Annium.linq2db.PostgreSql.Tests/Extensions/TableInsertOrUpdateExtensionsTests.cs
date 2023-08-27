using System.Threading.Tasks;
using Annium.linq2db.Tests.Lib.Extensions;
using Xunit;
using Xunit.Abstractions;

namespace Annium.linq2db.PostgreSql.Tests.Extensions;

public class TableInsertOrUpdateExtensionsTests : TableInsertOrUpdateExtensionsTestsBase
{
    public TableInsertOrUpdateExtensionsTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        AddServicePack<ServicePack>();
    }

    [Fact]
    public async Task Insert()
    {
        await Insert_Base();
    }

    [Fact]
    public async Task Update()
    {
        await Update_Base();
    }

    [Fact]
    public async Task InsertOrUpdate()
    {
        await InsertOrUpdate_Base();
    }
}