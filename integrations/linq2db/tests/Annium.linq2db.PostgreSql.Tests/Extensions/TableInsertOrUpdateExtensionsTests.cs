using System.Threading.Tasks;
using Annium.linq2db.Tests.Lib.Extensions;
using Xunit;

namespace Annium.linq2db.PostgreSql.Tests.Extensions;

/// <summary>
/// Tests for table insert or update extension methods with PostgreSQL backend
/// </summary>
public class TableInsertOrUpdateExtensionsTests : TableInsertOrUpdateExtensionsTestsBase
{
    /// <summary>
    /// Initializes a new instance of the TableInsertOrUpdateExtensionsTests class
    /// </summary>
    /// <param name="outputHelper">Test output helper for logging</param>
    public TableInsertOrUpdateExtensionsTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        RegisterServicePack<ServicePack>();
    }

    /// <summary>
    /// Tests insert operation using PostgreSQL backend
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task Insert()
    {
        await Insert_Base();
    }

    /// <summary>
    /// Tests update operation using PostgreSQL backend
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task Update()
    {
        await Update_Base();
    }

    /// <summary>
    /// Tests insert or update (upsert) operation using PostgreSQL backend
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task InsertOrUpdate()
    {
        await InsertOrUpdate_Base();
    }
}
