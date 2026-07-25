using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Cache.Abstractions;
using Annium.Core.Runtime.Time;
using Annium.Redis;
using Annium.Testing;
using NodaTime;
using Xunit;

namespace Annium.Cache.Redis.Tests;

/// <summary>
/// Redis-cache storage-timing race tests over a gated in-process <see cref="IRedisStorage"/> double
/// (<see cref="GatedRedisStorage"/>), which lets a concurrent <c>RemoveAsync</c> be interleaved with an
/// in-flight write at an exact point — impossible against a real Testcontainers backend that cannot pause
/// between a GET and a SET.
/// </summary>
public class RaceTests : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RaceTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper for logging test information.</param>
    public RaceTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        RegisterServicePack<FakeStorageServicePack>();
    }

    /// <summary>
    /// Verifies the sliding-refresh-vs-remove race: if a <c>RemoveAsync</c> lands while a live sliding entry's
    /// prolongation SET is in flight, the remove must still win — the refreshed entry must not resurrect the key.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task GetOrCreateAsync_SlidingRefreshRacesRemove_KeyStaysDeleted()
    {
        // arrange
        var ct = TestContext.Current.CancellationToken;
        Get<ITimeProviderSwitcher>().UseManagedTime();
        Get<ITimeManager>().SetNow(SystemClock.Instance.GetCurrentInstant());
        var cache = Get<ICache<Guid, string>>();
        var storage = (GatedRedisStorage)Get<IRedisStorage>();
        var key = Guid.NewGuid();
        var prefixed = RedisTestKeys.Prefixed<string>(key);
        var options = CacheOptions.WithSlidingExpiration(Duration.FromMinutes(1));

        var factoryCalls = 0;
        ValueTask<string> Factory(Guid k, CancellationToken token)
        {
            factoryCalls++;

            return ValueTask.FromResult("v");
        }

        // seed a live sliding entry by letting the cache create it (ungated)
        (await cache.GetOrCreateAsync(key, Factory, options, ct)).Is("v");
        storage.Has(prefixed).IsTrue();

        // arm the gate so the NEXT SET (the sliding-refresh prolongation) pauses mid-flight
        var refreshEntered = storage.ArmSetGate();

        // act: hit the live entry — the winner reads (hit) then starts the sliding-refresh SET, which now pauses
        var getTask = cache.GetOrCreateAsync(key, Factory, options, ct).AsTask();
        await refreshEntered.WaitAsync(TimeSpan.FromSeconds(5), ct);

        // a concurrent remove lands while the refresh SET is paused
        await cache.RemoveAsync(key, ct);

        // release the paused refresh SET (it would resurrect the key); the invalidation guard must delete it again
        storage.ReleaseSet();
        // getTask is created locally above via .AsTask(); awaiting it here is safe (not a foreign task)
#pragma warning disable VSTHRD003
        (await getTask.WaitAsync(TimeSpan.FromSeconds(5), ct)).Is("v");
#pragma warning restore VSTHRD003

        // assert: the remove wins — the key does not survive the refresh; the second call was a hit (no factory)
        storage.Has(prefixed).IsFalse();
        factoryCalls.Is(1);
    }

    /// <summary>
    /// Verifies the post-write invalidation compensating delete: if a <c>RemoveAsync</c> lands after the
    /// pre-write invalidation check but before the create-path SET completes, the just-written entry is
    /// compensating-deleted so the remove is not lost.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task GetOrCreateAsync_PostWriteInvalidation_CompensatingDeleteRemovesEntry()
    {
        // arrange
        var ct = TestContext.Current.CancellationToken;
        Get<ITimeProviderSwitcher>().UseManagedTime();
        Get<ITimeManager>().SetNow(SystemClock.Instance.GetCurrentInstant());
        var cache = Get<ICache<Guid, string>>();
        var storage = (GatedRedisStorage)Get<IRedisStorage>();
        var key = Guid.NewGuid();
        var prefixed = RedisTestKeys.Prefixed<string>(key);
        var options = CacheOptions.WithSlidingExpiration(Duration.FromMinutes(1));

        // arm the gate so the create-path SET pauses AFTER the factory produces, BEFORE the write lands
        var setEntered = storage.ArmSetGate();

        // act: miss → factory → SET (paused)
        var getTask = cache.GetOrCreateAsync(key, static (_, _) => ValueTask.FromResult("v"), options, ct).AsTask();
        await setEntered.WaitAsync(TimeSpan.FromSeconds(5), ct);

        // remove lands after the pre-write invalidation check but before the SET completes
        await cache.RemoveAsync(key, ct);

        // release the create SET (writes the entry); the post-write guard must compensating-delete it
        storage.ReleaseSet();
        // getTask is created locally above via .AsTask(); awaiting it here is safe (not a foreign task)
#pragma warning disable VSTHRD003
        (await getTask.WaitAsync(TimeSpan.FromSeconds(5), ct)).Is("v");
#pragma warning restore VSTHRD003

        // assert: the entry is purged despite the write landing after the remove
        storage.Has(prefixed).IsFalse();
    }

    /// <summary>
    /// Verifies the physical-TTL clamp: an absolute expiration at "now" yields a non-positive raw TTL that must
    /// be clamped to a small POSITIVE duration before hitting storage — never <see cref="Duration.Zero"/>, which
    /// the real backend maps to "no expiration" (a leaked never-expiring key). Asserts the ttl actually passed to
    /// storage, so removing the clamp fails the test (logical expiry alone would not catch it).
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task GetOrCreateAsync_AbsoluteExpirationAtNow_ClampsPhysicalTtlToPositive()
    {
        // arrange
        var ct = TestContext.Current.CancellationToken;
        Get<ITimeProviderSwitcher>().UseManagedTime();
        Get<ITimeManager>().SetNow(SystemClock.Instance.GetCurrentInstant());
        var timeProvider = Get<ITimeProvider>();
        var cache = Get<ICache<Guid, string>>();
        var storage = (GatedRedisStorage)Get<IRedisStorage>();
        var key = Guid.NewGuid();

        var calls = 0;
        ValueTask<string> Factory(Guid k, CancellationToken token)
        {
            Interlocked.Increment(ref calls);

            return ValueTask.FromResult("v");
        }

        // act: absolute expiration AT now → GetExpiresAt(now) == now → raw ttl == 0 → must be clamped to 1ms
        var options = CacheOptions.WithAbsoluteExpiration(timeProvider.Now);
        (await cache.GetOrCreateAsync(key, Factory, options, ct)).Is("v");

        // assert: the physical ttl the cache sent to storage is a small positive duration, never Zero/negative
        (storage.LastSetExpires > Duration.Zero).IsTrue();
        storage.LastSetExpires.Is(Duration.FromMilliseconds(1));

        // and: logically expired immediately (now >= ExpiresAtMs) → next call re-invokes the factory
        (await cache.GetOrCreateAsync(key, Factory, options, ct)).Is("v");
        calls.Is(2);
    }

    /// <summary>
    /// Verifies the value-type discriminator prevents cross-type key collision: two distinct closed generics
    /// (<c>ICache&lt;Guid,string&gt;</c> and <c>ICache&lt;Guid,int&gt;</c>) resolved from one registration and
    /// sharing a single Redis store must not collide on an identical key value — each keeps its own value, and a
    /// remove on one value-type does not evict the other. A regression that dropped the discriminator would fail
    /// this (both would map to <c>test:{guid}</c>).
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task GetOrCreateAsync_DistinctValueTypesSameKey_DoNotCollide()
    {
        // arrange
        var ct = TestContext.Current.CancellationToken;
        Get<ITimeProviderSwitcher>().UseManagedTime();
        Get<ITimeManager>().SetNow(SystemClock.Instance.GetCurrentInstant());
        var strings = Get<ICache<Guid, string>>();
        var ints = Get<ICache<Guid, int>>();
        var options = CacheOptions.WithSlidingExpiration(Duration.FromMinutes(1));
        var key = Guid.NewGuid();

        // act: same TKey value, two different TValue caches over one shared store
        (await strings.GetOrCreateAsync(key, static (_, _) => ValueTask.FromResult("v"), options, ct)).Is("v");
        (await ints.GetOrCreateAsync(key, static (_, _) => ValueTask.FromResult(42), options, ct)).Is(42);

        // assert: no collision — each cache still returns its own value (factory must not re-run on a hit)
        (await strings.GetOrCreateAsync(key, static (_, _) => ValueTask.FromResult("other"), options, ct)).Is("v");
        (await ints.GetOrCreateAsync(key, static (_, _) => ValueTask.FromResult(-1), options, ct)).Is(42);

        // and: removing one value-type's entry does not evict the other's
        await ints.RemoveAsync(key, ct);
        (await strings.GetOrCreateAsync(key, static (_, _) => ValueTask.FromResult("other"), options, ct)).Is("v");
    }

    /// <summary>
    /// Verifies the physical-TTL leak-guard for the ordinary (positive remaining-lifetime) case: the ttl the
    /// cache sends to storage equals the full remaining lifetime (expiresAt - now), not the near-immediate 1ms
    /// clamp. Guards against a regression that always clamps or computes the ttl with the wrong operands/sign
    /// (which would leak every key with a 1ms TTL) — invisible to logical-expiry-only assertions.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task GetOrCreateAsync_PositiveLifetime_SetsPhysicalTtlToRemainingLifetime()
    {
        // arrange
        var ct = TestContext.Current.CancellationToken;
        Get<ITimeProviderSwitcher>().UseManagedTime();
        Get<ITimeManager>().SetNow(SystemClock.Instance.GetCurrentInstant());
        var cache = Get<ICache<Guid, string>>();
        var storage = (GatedRedisStorage)Get<IRedisStorage>();
        var key = Guid.NewGuid();
        var lifetime = Duration.FromMinutes(1);

        // act: sliding 60s at fixed managed now → expiresAt = now + 60s → physical ttl == 60s (no clamp)
        (
            await cache.GetOrCreateAsync(
                key,
                static (_, _) => ValueTask.FromResult("v"),
                CacheOptions.WithSlidingExpiration(lifetime),
                ct
            )
        ).Is("v");

        // assert: the leak-guard ttl carries the full remaining lifetime, not the degenerate 1ms clamp
        storage.LastSetExpires.Is(lifetime);
    }
}
