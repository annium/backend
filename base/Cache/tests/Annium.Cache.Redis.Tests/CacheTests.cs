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

    // ── Redis-specific harness / white-box ──────────────────────────────────────────────

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

        var cache = Get<ICache<Guid, string>>();
        cache.IsNotNull();

        Get<RedisCacheOptions>().KeyPrefix.Is("test:");

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

        await storage.SetAsync(prefixed, "v", ct: ct);
        (await storage.GetAsync(prefixed, ct)).IsNotDefault();

        await cache.RemoveAsync(key, ct);

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

    // ── Shared contract scenarios (Task 3: core + Absolute + single-flight) ──────────────

    /// <summary>
    /// Tests the default behavior of GetOrCreateAsync (concurrent callers share one factory run, value-equal).
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task GetOrCreateAsync_Default()
    {
        await GetOrCreateAsync_Default_Base();
    }

    /// <summary>
    /// Tests GetOrCreateAsync with absolute expiration (logical expiry drives re-creation after managed time advances).
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task GetOrCreateAsync_AbsoluteExpiration()
    {
        await GetOrCreateAsync_AbsoluteExpiration_Base();
    }

    /// <summary>
    /// Tests the RemoveAsync functionality (remove → next GetOrCreate re-invokes the factory).
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task RemoveAsync()
    {
        await RemoveAsync_Base();
    }

    /// <summary>
    /// Verifies removing a non-existent key is a silent no-op.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task RemoveAsync_NonExistentKey_CompletesWithoutException()
    {
        await RemoveAsync_NonExistentKey_CompletesWithoutException_Base();
    }

    /// <summary>
    /// Verifies the context-carrying GetOrCreateAsync overload forwards the supplied context to the factory.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task GetOrCreateAsync_WithContext_ContextPassedToFactory()
    {
        await GetOrCreateAsync_WithContext_ContextPassedToFactory_Base();
    }

    /// <summary>
    /// Verifies cache operations on a disposed cache throw instead of hanging.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task GetOrCreateAsync_AfterDispose_Throws()
    {
        await GetOrCreateAsync_AfterDispose_Throws_Base();
    }

    /// <summary>
    /// Verifies a pre-cancelled CT is observed before the factory is invoked.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task GetOrCreateAsync_PreCancelledCt_ThrowsBeforeFactoryCall()
    {
        await GetOrCreateAsync_PreCancelledCt_ThrowsBeforeFactoryCall_Base();
    }

    /// <summary>
    /// Verifies RemoveAsync observes a pre-cancelled CT.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task RemoveAsync_PreCancelledCt_Throws()
    {
        await RemoveAsync_PreCancelledCt_Throws_Base();
    }

    /// <summary>
    /// Verifies DisposeAsync is idempotent.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task DisposeAsync_CalledTwice_DoesNotThrow()
    {
        await DisposeAsync_CalledTwice_DoesNotThrow_Base();
    }

    /// <summary>
    /// Verifies concurrent access across many distinct keys runs each key's factory exactly once.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task GetOrCreateAsync_ConcurrentDistinctKeys_EachFactoryOnce()
    {
        await GetOrCreateAsync_ConcurrentDistinctKeys_EachFactoryOnce_Base();
    }

    // ── Sliding + FirstWriterWins (Task 4) ──────────────────────────────────────────────

    /// <summary>
    /// Tests GetOrCreateAsync with sliding expiration (recreated after the window elapses without access).
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task GetOrCreateAsync_SlidingExpiration()
    {
        await GetOrCreateAsync_SlidingExpiration_Base();
    }

    /// <summary>
    /// Verifies sliding expiration is prolonged on each hit so an accessed entry survives its original window.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task GetOrCreateAsync_SlidingExpiration_AccessWithinWindowProlongs()
    {
        await GetOrCreateAsync_SlidingExpiration_AccessWithinWindowProlongs_Base();
    }

    /// <summary>
    /// Verifies first-writer-wins: a later caller's different (shorter) options are ignored for a live key.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task GetOrCreateAsync_FirstWriterWins_LaterCallerOptionsIgnored()
    {
        await GetOrCreateAsync_FirstWriterWins_LaterCallerOptionsIgnored_Base();
    }

    // ── Single-flight edges: poison / cancel / drain (Task 5) ────────────────────────────

    /// <summary>
    /// Verifies a factory exception surfaces to the awaiting caller and the slot is unpoisoned.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task GetOrCreateAsync_FactoryThrows_PropagatesAndUnpoisons()
    {
        await GetOrCreateAsync_FactoryThrows_PropagatesAndUnpoisons_Base();
    }

    /// <summary>
    /// Verifies all concurrent callers observe the deduplicated factory exception (no hang).
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task GetOrCreateAsync_FactoryThrows_ConcurrentCallersAllSeeException()
    {
        await GetOrCreateAsync_FactoryThrows_ConcurrentCallersAllSeeException_Base();
    }

    /// <summary>
    /// Verifies one caller's cancellation does not fault the shared task for other awaiters.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task GetOrCreateAsync_CancelDuringFactory_OneAwaiterCancelsOthersContinue()
    {
        await GetOrCreateAsync_CancelDuringFactory_OneAwaiterCancelsOthersContinue_Base();
    }

    /// <summary>
    /// Verifies RemoveAsync during an in-flight factory delivers the value then re-invokes on the next call.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task RemoveAsync_WhileFactoryInFlight_CallerGetsValueThenReinvokes()
    {
        await RemoveAsync_WhileFactoryInFlight_CallerGetsValueThenReinvokes_Base();
    }

    /// <summary>
    /// Verifies disposing while a factory is in-flight drains cleanly and the caller still completes.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task DisposeAsync_WhileFactoryInFlight_DrainsAndCallerCompletes()
    {
        await DisposeAsync_WhileFactoryInFlight_DrainsAndCallerCompletes_Base();
    }
}
