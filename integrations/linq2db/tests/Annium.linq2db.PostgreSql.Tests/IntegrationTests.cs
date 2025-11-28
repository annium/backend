using System.Threading.Tasks;
using Annium.linq2db.Tests.Lib;
using Xunit;

namespace Annium.linq2db.PostgreSql.Tests;

/// <summary>
/// Integration tests for linq2db PostgreSQL functionality including end-to-end operations and performance testing
/// </summary>
public class IntegrationTests : IntegrationTestsBase
{
    /// <summary>
    /// Initializes a new instance of the IntegrationTests class
    /// </summary>
    /// <param name="outputHelper">Test output helper for logging</param>
    public IntegrationTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        RegisterServicePack<ServicePack>();
    }

    /// <summary>
    /// Tests complete end-to-end functionality with PostgreSQL backend
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task EndToEnd()
    {
        await EndToEnd_Base();
    }

    /// <summary>
    /// Tests high-load insert operations performance with PostgreSQL backend
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task HighLoad_Insert()
    {
        await HighLoad_Insert_Base();
    }

    /// <summary>
    /// Tests high-load select operations performance with PostgreSQL backend
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task HighLoad_Select()
    {
        await HighLoad_Select_Base();
    }
}
