using System.Threading.Tasks;
using Annium.Cache.Tests.Lib;
using Xunit;

namespace Annium.Cache.Redis.Tests;

/// <summary>
/// Tests for the Redis cache implementation.
/// </summary>
public class CacheTests : CacheTestsBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CacheTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper for logging test information.</param>
    public CacheTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        AddServicePack<ServicePack>();
    }

    /// <summary>
    /// Tests the default behavior of GetOrCreateAsync for the Redis cache implementation.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact(Skip = "not implemented")]
    public async Task GetOrCreateAsync_Default()
    {
        await GetOrCreateAsync_Default_Base();
    }

    /// <summary>
    /// Tests GetOrCreateAsync with absolute expiration for the Redis cache implementation.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact(Skip = "not implemented")]
    public async Task GetOrCreateAsync_AbsoluteExpiration()
    {
        await GetOrCreateAsync_AbsoluteExpiration_Base();
    }

    /// <summary>
    /// Tests GetOrCreateAsync with sliding expiration for the Redis cache implementation.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact(Skip = "not implemented")]
    public async Task GetOrCreateAsync_SlidingExpiration()
    {
        await GetOrCreateAsync_SlidingExpiration_Base();
    }

    /// <summary>
    /// Tests the RemoveAsync functionality for the Redis cache implementation.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact(Skip = "not implemented")]
    public async Task RemoveAsync()
    {
        await RemoveAsync_Base();
    }
}
