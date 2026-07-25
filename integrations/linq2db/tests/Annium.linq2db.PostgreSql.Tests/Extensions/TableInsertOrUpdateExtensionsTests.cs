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

    /// <summary>
    /// Tests created-only insert timestamp handling using PostgreSQL backend
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task Insert_CreatedOnly()
    {
        await Insert_CreatedOnly_Base();
    }

    /// <summary>
    /// Tests created-only upsert timestamp handling using PostgreSQL backend
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task InsertOrUpdate_CreatedOnly()
    {
        await InsertOrUpdate_CreatedOnly_Base();
    }

    /// <summary>
    /// Tests manual-timestamp insert/update through the auto pipeline using PostgreSQL backend
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task Insert_ManualTime()
    {
        await Insert_ManualTime_Base();
    }

    /// <summary>
    /// Tests composite-primary-key update targeting using PostgreSQL backend
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task Update_CompositeKey()
    {
        await Update_CompositeKey_Base();
    }

    /// <summary>
    /// Tests that updating a table with no primary key throws using PostgreSQL backend
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task Update_NoPrimaryKey_Throws()
    {
        await Update_NoPrimaryKey_Throws_Base();
    }
}
