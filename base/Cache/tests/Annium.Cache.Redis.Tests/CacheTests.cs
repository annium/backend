using System;
using System.Threading.Tasks;
using Annium.Cache.Abstractions;
using Annium.Cache.Tests.Lib;
using Annium.Core.DependencyInjection;
using Annium.Redis;
using Annium.Testing;
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
        RegisterServicePack<ServicePack>();
    }

    /// <summary>
    /// Verifies the DI/test harness: the Testcontainers Redis backend starts, the cache and the shared
    /// <see cref="IRedisStorage"/> resolve, the cache options are registered, and the backend is reachable
    /// (a storage round-trip succeeds).
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task Resolves_CacheAndContainerStarts()
    {
        var ct = TestContext.Current.CancellationToken;

        // cache resolves from the open-generic registration
        var cache = Get<ICache<Guid, string>>();
        cache.IsNotNull();

        // cache options are registered with the configured prefix
        Get<RedisCacheOptions>().KeyPrefix.Is("test:");

        // the shared storage is reachable end-to-end (round-trip against the started container)
        var storage = Get<IRedisStorage>();
        var key = Guid.NewGuid().ToString();
        await storage.SetAsync(key, "v", ct: ct);
        (await storage.GetAsync(key, ct)).Is("v");
    }

    /// <summary>
    /// Verifies <see cref="ICache{TKey,TValue}.RemoveAsync"/> deletes the prefixed key via the shared storage.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task RemoveAsync_DeletesPrefixedKeyViaStorage()
    {
        var ct = TestContext.Current.CancellationToken;
        var cache = Get<ICache<Guid, string>>();
        var storage = Get<IRedisStorage>();
        var key = Guid.NewGuid();
        var prefixed = $"test:{key}";

        // seed the prefixed entry directly via storage
        await storage.SetAsync(prefixed, "v", ct: ct);
        (await storage.GetAsync(prefixed, ct)).IsNotDefault();

        // act
        await cache.RemoveAsync(key, ct);

        // assert: the prefixed key was deleted
        (await storage.GetAsync(prefixed, ct)).IsDefault();
    }

    /// <summary>
    /// Verifies the configure overload of <c>AddRedisCache</c> builds and registers
    /// <see cref="RedisCacheOptions"/> with the supplied prefix (DI-only, no container).
    /// </summary>
    [Fact]
    public void AddRedisCache_ConfigureOverload_RegistersOptions()
    {
        var container = new ServiceContainer();
        container.AddRedisCache(cfg => cfg.KeyPrefix = "t:");

        var provider = container.BuildServiceProvider();

        provider.Resolve<RedisCacheOptions>().KeyPrefix.Is("t:");
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
