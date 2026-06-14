using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Cache.Abstractions;
using Annium.Core.Runtime.Time;
using Annium.Testing;
using NodaTime;
using Xunit;

namespace Annium.Cache.Tests.Lib;

/// <summary>
/// Base class providing common test scenarios for cache implementations.
/// </summary>
public class CacheTestsBase : TestBase
{
    /// <summary>
    /// The number of concurrent callers used by the deduplication / concurrency scenarios.
    /// </summary>
    private const int ConcurrencyCount = 1000;

    /// <summary>
    /// Counter to track the number of times the factory method has been called.
    /// </summary>
    private int _factoryCounter;

    /// <summary>
    /// Initializes a new instance of the <see cref="CacheTestsBase"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper for logging test information.</param>
    protected CacheTestsBase(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Tests the default behavior of GetOrCreateAsync to ensure concurrent calls for the same key return the same cached instance.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    protected async Task GetOrCreateAsync_Default_Base()
    {
        // arrange
        Get<ITimeProviderSwitcher>().UseManagedTime();
        var cache = Get<ICache<Guid, Page>>();
        var key = Guid.NewGuid();
        var options = CacheOptions.WithSlidingExpiration(Duration.FromMinutes(1));
        var ct = TestContext.Current.CancellationToken;
        var count = ConcurrencyCount;

        // act
        var items = await Task.WhenAll(
            Enumerable.Range(0, count).Select(async _ => await cache.GetOrCreateAsync(key, GetPageAsync, options, ct))
        );

        // assert
        EnsureItems(1, key, count, items);
    }

    /// <summary>
    /// Tests cache behavior with absolute expiration to ensure items expire at the specified time and are recreated when accessed after expiration.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    protected async Task GetOrCreateAsync_AbsoluteExpiration_Base()
    {
        // arrange
        Get<ITimeProviderSwitcher>().UseManagedTime();
        var cache = Get<ICache<Guid, Page>>();
        var timeManager = Get<ITimeManager>();
        timeManager.SetNow(SystemClock.Instance.GetCurrentInstant());
        var timeProvider = Get<ITimeProvider>();
        var expiresAt = timeProvider.Now + Duration.FromMinutes(1);
        var key = Guid.NewGuid();
        var ct = TestContext.Current.CancellationToken;

        var options1 = CacheOptions.WithAbsoluteExpiration(expiresAt);
        var count = ConcurrencyCount;

        // act
        var items = await Task.WhenAll(
            Enumerable.Range(0, count).Select(async _ => await cache.GetOrCreateAsync(key, GetPageAsync, options1, ct))
        );

        // assert
        EnsureItems(1, key, count, items);

        // arrange
        timeManager.SetNow(expiresAt);
        expiresAt = timeProvider.Now + Duration.FromMinutes(1);
        var options2 = CacheOptions.WithAbsoluteExpiration(expiresAt);

        // act
        items = await Task.WhenAll(
            Enumerable.Range(0, count).Select(async _ => await cache.GetOrCreateAsync(key, GetPageAsync, options2, ct))
        );

        // assert
        EnsureItems(2, key, count, items);
    }

    /// <summary>
    /// Tests cache behavior with sliding expiration to ensure items expire after the specified duration of inactivity and are recreated when accessed after expiration.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    protected async Task GetOrCreateAsync_SlidingExpiration_Base()
    {
        // arrange
        Get<ITimeProviderSwitcher>().UseManagedTime();
        var cache = Get<ICache<Guid, Page>>();
        var timeManager = Get<ITimeManager>();
        timeManager.SetNow(SystemClock.Instance.GetCurrentInstant());
        var timeProvider = Get<ITimeProvider>();
        var lifetime = Duration.FromMinutes(1);
        var key = Guid.NewGuid();
        var ct = TestContext.Current.CancellationToken;

        var options = CacheOptions.WithSlidingExpiration(lifetime);
        var count = ConcurrencyCount;

        // act
        var items = await Task.WhenAll(
            Enumerable.Range(0, count).Select(async _ => await cache.GetOrCreateAsync(key, GetPageAsync, options, ct))
        );

        // assert
        EnsureItems(1, key, count, items);

        // arrange
        timeManager.SetNow(timeProvider.Now + lifetime);

        // act
        items = await Task.WhenAll(
            Enumerable.Range(0, count).Select(async _ => await cache.GetOrCreateAsync(key, GetPageAsync, options, ct))
        );

        // assert
        EnsureItems(2, key, count, items);
    }

    /// <summary>
    /// Tests cache removal functionality to ensure items are properly removed and recreated when accessed again.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    protected async Task RemoveAsync_Base()
    {
        // arrange
        Get<ITimeProviderSwitcher>().UseManagedTime();
        var cache = Get<ICache<Guid, Page>>();
        var timeManager = Get<ITimeManager>();
        timeManager.SetNow(SystemClock.Instance.GetCurrentInstant());
        var lifetime = Duration.FromMinutes(1);
        var key = Guid.NewGuid();
        var ct = TestContext.Current.CancellationToken;

        var options = CacheOptions.WithSlidingExpiration(lifetime);
        var count = ConcurrencyCount;

        // act
        var items = await Task.WhenAll(
            Enumerable.Range(0, count).Select(async _ => await cache.GetOrCreateAsync(key, GetPageAsync, options, ct))
        );

        // assert
        EnsureItems(1, key, count, items);

        // act
        await cache.RemoveAsync(key, ct);

        // act
        items = await Task.WhenAll(
            Enumerable.Range(0, count).Select(async _ => await cache.GetOrCreateAsync(key, GetPageAsync, options, ct))
        );

        // assert
        EnsureItems(2, key, count, items);
    }

    /// <summary>
    /// Verifies Fix 1: factory exceptions surface to the awaiting caller AND the poisoned slot is removed
    /// so a subsequent call retries with a fresh factory.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    protected async Task GetOrCreateAsync_FactoryThrows_PropagatesAndUnpoisons_Base()
    {
        // arrange
        Get<ITimeProviderSwitcher>().UseManagedTime();
        var cache = Get<ICache<Guid, Page>>();
        var key = Guid.NewGuid();
        var options = CacheOptions.WithSlidingExpiration(Duration.FromMinutes(1));
        var ct = TestContext.Current.CancellationToken;

        // act + assert: first call faults with the factory exception
        var ex = await Wrap.It(async () =>
                await cache.GetOrCreateAsync(
                    key,
                    static (_, _) => ValueTask.FromException<Page>(new InvalidOperationException("boom")),
                    options,
                    ct
                )
            )
            .ThrowsAsync<InvalidOperationException>();
        ex.Message.Is("boom");

        // act: second call with non-throwing factory succeeds (slot unpoisoned)
        var page = await cache.GetOrCreateAsync(key, GetPageAsync, options, ct);

        // assert
        page.Is(new Page(key));
    }

    /// <summary>
    /// Verifies Fix 1 under concurrency: when the deduplicated factory call throws, ALL awaiting callers
    /// surface the exception (none hangs).
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    protected async Task GetOrCreateAsync_FactoryThrows_ConcurrentCallersAllSeeException_Base()
    {
        // arrange
        Get<ITimeProviderSwitcher>().UseManagedTime();
        var cache = Get<ICache<Guid, Page>>();
        var key = Guid.NewGuid();
        var options = CacheOptions.WithSlidingExpiration(Duration.FromMinutes(1));
        var ct = TestContext.Current.CancellationToken;
        var count = ConcurrencyCount;

        // act: 1000 concurrent callers, all should see the factory exception
        var tasks = Enumerable
            .Range(0, count)
            .Select(async _ =>
            {
                try
                {
                    await cache.GetOrCreateAsync(
                        key,
                        static (_, _) => ValueTask.FromException<Page>(new InvalidOperationException("boom")),
                        options,
                        ct
                    );
                    return false;
                }
                catch (InvalidOperationException)
                {
                    return true;
                }
            });

        var results = await Task.WhenAll(tasks).WaitAsync(TimeSpan.FromSeconds(5), ct);

        // assert
        results.Has(count);
        results.All(r => r).IsTrue();
    }

    /// <summary>
    /// Verifies Fix 2: DisposeAsync is idempotent — a second call is a no-op and does not throw.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    protected async Task DisposeAsync_CalledTwice_DoesNotThrow_Base()
    {
        // arrange
        var cache = Get<ICache<Guid, Page>>();
        var disposable = (IAsyncDisposable)cache;

        // act + assert (first dispose succeeds; second is a no-op and must not throw)
        await disposable.DisposeAsync();
        await disposable.DisposeAsync();
    }

    /// <summary>
    /// Verifies Fix 3: a pre-cancelled CT is observed before the factory is invoked.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    protected async Task GetOrCreateAsync_PreCancelledCt_ThrowsBeforeFactoryCall_Base()
    {
        // arrange
        Get<ITimeProviderSwitcher>().UseManagedTime();
        var cache = Get<ICache<Guid, Page>>();
        var key = Guid.NewGuid();
        var options = CacheOptions.WithSlidingExpiration(Duration.FromMinutes(1));
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // act + assert
        await Wrap.It(async () => await cache.GetOrCreateAsync(key, GetPageAsync, options, cts.Token))
            .ThrowsAsync<OperationCanceledException>();

        _factoryCounter.Is(0);
    }

    /// <summary>
    /// Verifies Fix 3: per-caller cancellation does not fault the shared TCS — other callers awaiting the
    /// same key continue to receive the value when the factory completes.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    protected async Task GetOrCreateAsync_CancelDuringFactory_OneAwaiterCancelsOthersContinue_Base()
    {
        // arrange
        Get<ITimeProviderSwitcher>().UseManagedTime();
        var cache = Get<ICache<Guid, Page>>();
        var key = Guid.NewGuid();
        var options = CacheOptions.WithSlidingExpiration(Duration.FromMinutes(1));
        var ct = TestContext.Current.CancellationToken;

        var factoryEntered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var factoryGate = new TaskCompletionSource<Page>(TaskCreationOptions.RunContinuationsAsynchronously);
        ValueTask<Page> SlowFactory(Guid k, CancellationToken token)
        {
            factoryEntered.TrySetResult();

            return new(factoryGate.Task);
        }

        using var cts1 = new CancellationTokenSource();

        var task1 = cache.GetOrCreateAsync(key, SlowFactory, options, cts1.Token).AsTask();
        var task2 = cache.GetOrCreateAsync(key, SlowFactory, options, ct).AsTask();

        // wait until the deduplicated factory has actually started (both callers are then registered on the
        // shared TCS) — a deterministic signal rather than a timing delay
        await factoryEntered.Task.WaitAsync(TimeSpan.FromSeconds(5), ct);

        // act 1: cancel caller 1 — its await should throw, factory continues
        await cts1.CancelAsync();
        // task1 is created locally above via .AsTask(); awaiting it here is safe (not a foreign task)
#pragma warning disable VSTHRD003
        await Wrap.It(async () => await task1).ThrowsAsync<OperationCanceledException>();
#pragma warning restore VSTHRD003

        // act 2: release the factory; caller 2 receives the value
        factoryGate.TrySetResult(new Page(key));
        var result = await task2.WaitAsync(TimeSpan.FromSeconds(5), ct);

        // assert
        result.Is(new Page(key));
    }

    /// <summary>
    /// Verifies Fix 3: RemoveAsync observes a pre-cancelled CT.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    protected async Task RemoveAsync_PreCancelledCt_Throws_Base()
    {
        // arrange
        var cache = Get<ICache<Guid, Page>>();
        var key = Guid.NewGuid();
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // act + assert
        await Wrap.It(async () => await cache.RemoveAsync(key, cts.Token)).ThrowsAsync<OperationCanceledException>();
    }

    /// <summary>
    /// Verifies sliding expiration is PROLONGED on each hit: an entry accessed within its window survives
    /// past its original expiry, because each access pushes ExpiresAt forward by the lifetime.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    protected async Task GetOrCreateAsync_SlidingExpiration_AccessWithinWindowProlongs_Base()
    {
        // arrange
        Get<ITimeProviderSwitcher>().UseManagedTime();
        var cache = Get<ICache<Guid, Page>>();
        var timeManager = Get<ITimeManager>();
        timeManager.SetNow(SystemClock.Instance.GetCurrentInstant());
        var timeProvider = Get<ITimeProvider>();
        var lifetime = Duration.FromMinutes(1);
        var key = Guid.NewGuid();
        var ct = TestContext.Current.CancellationToken;
        var options = CacheOptions.WithSlidingExpiration(lifetime);

        // act: create (factory runs once)
        var first = await cache.GetOrCreateAsync(key, GetPageAsync, options, ct);

        // act: access within the window (50s of 60s) — prolongs ExpiresAt to now+60s (= origin+110s)
        timeManager.SetNow(timeProvider.Now + Duration.FromSeconds(50));
        var second = await cache.GetOrCreateAsync(key, GetPageAsync, options, ct);

        // act: advance past the ORIGINAL window (origin+80s > 60s) but within the prolonged window (< origin+110s)
        timeManager.SetNow(timeProvider.Now + Duration.FromSeconds(30));
        var third = await cache.GetOrCreateAsync(key, GetPageAsync, options, ct);

        // assert: the item survived via prolongation — factory ran exactly once, same instance throughout
        _factoryCounter.Is(1);
        ReferenceEquals(second, first).IsTrue();
        ReferenceEquals(third, first).IsTrue();
    }

    /// <summary>
    /// Verifies first-writer-wins option semantics: a later caller that supplies DIFFERENT (shorter) options
    /// for a live key is ignored — the entry keeps the options it was created with and is not evicted early.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    protected async Task GetOrCreateAsync_FirstWriterWins_LaterCallerOptionsIgnored_Base()
    {
        // arrange
        Get<ITimeProviderSwitcher>().UseManagedTime();
        var cache = Get<ICache<Guid, Page>>();
        var timeManager = Get<ITimeManager>();
        timeManager.SetNow(SystemClock.Instance.GetCurrentInstant());
        var timeProvider = Get<ITimeProvider>();
        var key = Guid.NewGuid();
        var ct = TestContext.Current.CancellationToken;

        // act: create with a LONG sliding window
        var longOptions = CacheOptions.WithSlidingExpiration(Duration.FromMinutes(5));
        var first = await cache.GetOrCreateAsync(key, GetPageAsync, longOptions, ct);

        // act: a later caller supplies a SHORT window — first-writer-wins ignores it, entry keeps the 5-min strategy
        var shortOptions = CacheOptions.WithSlidingExpiration(Duration.FromSeconds(1));
        var second = await cache.GetOrCreateAsync(key, GetPageAsync, shortOptions, ct);

        // act: advance past the SHORT window but within the LONG one
        timeManager.SetNow(timeProvider.Now + Duration.FromSeconds(30));
        var third = await cache.GetOrCreateAsync(key, GetPageAsync, shortOptions, ct);

        // assert: not evicted by the later caller's 1s window — factory ran once, same instance
        _factoryCounter.Is(1);
        ReferenceEquals(second, first).IsTrue();
        ReferenceEquals(third, first).IsTrue();
    }

    /// <summary>
    /// Verifies the post-factory expiry guard: when a slow factory completes AFTER its entry has expired,
    /// the cache discards the result (TrySetCanceled) and evicts the entry so a later call recreates it.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    protected async Task GetOrCreateAsync_FactoryCompletesAfterExpiry_CancelsAndEvicts_Base()
    {
        // arrange
        Get<ITimeProviderSwitcher>().UseManagedTime();
        var cache = Get<ICache<Guid, Page>>();
        var timeManager = Get<ITimeManager>();
        timeManager.SetNow(SystemClock.Instance.GetCurrentInstant());
        var timeProvider = Get<ITimeProvider>();
        var key = Guid.NewGuid();
        var ct = TestContext.Current.CancellationToken;
        var options = CacheOptions.WithSlidingExpiration(Duration.FromMinutes(1));

        var factoryGate = new TaskCompletionSource<Page>(TaskCreationOptions.RunContinuationsAsynchronously);
        ValueTask<Page> GatedFactory(Guid k, CancellationToken token) => new(factoryGate.Task);

        // act: start the gated factory (entry created with ExpiresAt = origin+60s), then advance past expiry
        var task = cache.GetOrCreateAsync(key, GatedFactory, options, ct).AsTask();
        timeManager.SetNow(timeProvider.Now + Duration.FromMinutes(2));

        // act: release the factory — the live-check sees the entry expired and cancels + evicts
        factoryGate.TrySetResult(new Page(key));
        // task is created locally above via .AsTask(); awaiting it here is safe (not a foreign task)
#pragma warning disable VSTHRD003
        await Wrap.It(async () => await task).ThrowsAsync<OperationCanceledException>();
#pragma warning restore VSTHRD003

        // assert: the entry was evicted — a fresh call invokes the (real) factory again and succeeds
        var fresh = await cache.GetOrCreateAsync(key, GetPageAsync, options, ct);
        fresh.Is(new Page(key));
        _factoryCounter.Is(1);
    }

    /// <summary>
    /// Verifies the context-carrying GetOrCreateAsync overload forwards the supplied context to the factory.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    protected async Task GetOrCreateAsync_WithContext_ContextPassedToFactory_Base()
    {
        // arrange
        Get<ITimeProviderSwitcher>().UseManagedTime();
        var cache = Get<ICache<Guid, Page>>();
        var key = Guid.NewGuid();
        var ct = TestContext.Current.CancellationToken;
        var options = CacheOptions.WithSlidingExpiration(Duration.FromMinutes(1));

        var context = $"ctx:{Guid.NewGuid()}";
        string? observed = null;
        ValueTask<Page> Factory(Guid k, string c, CancellationToken token)
        {
            observed = c;

            return ValueTask.FromResult(new Page(k));
        }

        // act: call the 3-arg context overload on ICache directly (not via the context-free extension)
        var page = await cache.GetOrCreateAsync(key, Factory, context, options, ct);

        // assert
        page.Is(new Page(key));
        observed.Is(context);
    }

    /// <summary>
    /// Verifies that cache operations on a disposed cache throw <see cref="ObjectDisposedException"/> rather than hang.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    protected async Task GetOrCreateAsync_AfterDispose_Throws_Base()
    {
        // arrange
        var cache = Get<ICache<Guid, Page>>();
        var key = Guid.NewGuid();
        var options = CacheOptions.WithSlidingExpiration(Duration.FromMinutes(1));
        var ct = TestContext.Current.CancellationToken;
        await ((IAsyncDisposable)cache).DisposeAsync();

        // act + assert: both operations observe disposal and throw instead of scheduling work that never completes
        await Wrap.It(async () => await cache.GetOrCreateAsync(key, GetPageAsync, options, ct))
            .ThrowsAsync<ObjectDisposedException>();
        await Wrap.It(async () => await cache.RemoveAsync(key, ct)).ThrowsAsync<ObjectDisposedException>();
    }

    /// <summary>
    /// Verifies that disposing while a factory is in-flight drains cleanly: the dispose completes and the
    /// awaiting caller receives its value without hanging.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    protected async Task DisposeAsync_WhileFactoryInFlight_DrainsAndCallerCompletes_Base()
    {
        // arrange
        Get<ITimeProviderSwitcher>().UseManagedTime();
        var cache = Get<ICache<Guid, Page>>();
        Get<ITimeManager>().SetNow(SystemClock.Instance.GetCurrentInstant());
        var key = Guid.NewGuid();
        var options = CacheOptions.WithSlidingExpiration(Duration.FromMinutes(1));
        var ct = TestContext.Current.CancellationToken;

        var factoryGate = new TaskCompletionSource<Page>(TaskCreationOptions.RunContinuationsAsynchronously);
        ValueTask<Page> GatedFactory(Guid k, CancellationToken token) => new(factoryGate.Task);

        // act: start a call whose factory is gated (in-flight on the executor), then begin disposing
        var task = cache.GetOrCreateAsync(key, GatedFactory, options, ct).AsTask();
        var disposeTask = ((IAsyncDisposable)cache).DisposeAsync();

        // release the factory so the in-flight work — and the executor drain — can complete
        factoryGate.TrySetResult(new Page(key));

        // assert: dispose drains (no deadlock) and the caller completes with its value (no hang)
        await disposeTask.AsTask().WaitAsync(TimeSpan.FromSeconds(5), ct);
        var caller = await task.WaitAsync(TimeSpan.FromSeconds(5), ct);
        caller.Is(new Page(key));
    }

    /// <summary>
    /// Verifies concurrent access across many DISTINCT keys: each key's factory runs exactly once and every
    /// caller receives the value for its own key.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    protected async Task GetOrCreateAsync_ConcurrentDistinctKeys_EachFactoryOnce_Base()
    {
        // arrange
        Get<ITimeProviderSwitcher>().UseManagedTime();
        var cache = Get<ICache<Guid, Page>>();
        var options = CacheOptions.WithSlidingExpiration(Duration.FromMinutes(1));
        var ct = TestContext.Current.CancellationToken;
        var keys = Enumerable.Range(0, 100).Select(_ => Guid.NewGuid()).ToArray();
        var callersPerKey = 10;

        // act: every key is requested by several concurrent callers at once
        var items = await Task.WhenAll(
            keys.SelectMany(key =>
                Enumerable
                    .Range(0, callersPerKey)
                    .Select(async _ => (key, page: await cache.GetOrCreateAsync(key, GetPageAsync, options, ct)))
            )
        );

        // assert: one factory invocation per distinct key; each caller got the page for its key
        _factoryCounter.Is(keys.Length);
        items.Has(keys.Length * callersPerKey);
        foreach (var (key, page) in items)
            page.Is(new Page(key));
    }

    /// <summary>
    /// Verifies that removing a key that was never inserted is a silent no-op (completes without throwing).
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    protected async Task RemoveAsync_NonExistentKey_CompletesWithoutException_Base()
    {
        // arrange
        var cache = Get<ICache<Guid, Page>>();
        var ct = TestContext.Current.CancellationToken;

        // act + assert: no exception for a missing key
        await cache.RemoveAsync(Guid.NewGuid(), ct);
    }

    /// <summary>
    /// Verifies the expiry-path eviction is identity-guarded: a slow factory that completes AFTER its entry
    /// expired AND after a replacement entry was created for the same key must NOT evict the replacement.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    protected async Task GetOrCreateAsync_ExpiredFactoryAfterReplacement_ReplacementSurvives_Base()
    {
        // arrange
        Get<ITimeProviderSwitcher>().UseManagedTime();
        var cache = Get<ICache<Guid, Page>>();
        var timeManager = Get<ITimeManager>();
        timeManager.SetNow(SystemClock.Instance.GetCurrentInstant());
        var timeProvider = Get<ITimeProvider>();
        var key = Guid.NewGuid();
        var ct = TestContext.Current.CancellationToken;
        var options = CacheOptions.WithSlidingExpiration(Duration.FromMinutes(1));

        var gate1 = new TaskCompletionSource<Page>(TaskCreationOptions.RunContinuationsAsynchronously);
        var gate2 = new TaskCompletionSource<Page>(TaskCreationOptions.RunContinuationsAsynchronously);
        ValueTask<Page> Factory1(Guid k, CancellationToken token) => new(gate1.Task);
        ValueTask<Page> Factory2(Guid k, CancellationToken token) => new(gate2.Task);

        // act: start factory1 (entry1, ExpiresAt = origin+60s), then advance past expiry
        var call1 = cache.GetOrCreateAsync(key, Factory1, options, ct).AsTask();
        timeManager.SetNow(timeProvider.Now + Duration.FromMinutes(2));

        // a second call now creates a REPLACEMENT entry2 (entry1 is expired → create path replaces _data[key])
        var call2 = cache.GetOrCreateAsync(key, Factory2, options, ct).AsTask();

        // release factory1: it completes expired AND after entry2 replaced it → its eviction must skip entry2
        gate1.TrySetResult(new Page(key));
        // call1/call2 are local tasks created above via .AsTask(); awaiting them here is safe (not foreign tasks)
#pragma warning disable VSTHRD003
        await Wrap.It(async () => await call1).ThrowsAsync<OperationCanceledException>();

        // release factory2: entry2 is still live → caller2 receives its value
        gate2.TrySetResult(new Page(key));
        var result2 = await call2.WaitAsync(TimeSpan.FromSeconds(5), ct);
#pragma warning restore VSTHRD003
        result2.Is(new Page(key));

        // assert: entry2 survived → a subsequent call returns the SAME cached instance without a new factory
        var subsequent = await cache.GetOrCreateAsync(key, GetPageAsync, options, ct);
        ReferenceEquals(subsequent, result2).IsTrue();
        _factoryCounter.Is(0);
    }

    /// <summary>
    /// Verifies the exception-path eviction is identity-guarded: a factory that throws AFTER its entry was
    /// removed and a replacement created for the same key must NOT evict the replacement.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    protected async Task GetOrCreateAsync_ThrowingFactoryAfterReplacement_ReplacementSurvives_Base()
    {
        // arrange
        Get<ITimeProviderSwitcher>().UseManagedTime();
        var cache = Get<ICache<Guid, Page>>();
        Get<ITimeManager>().SetNow(SystemClock.Instance.GetCurrentInstant());
        var key = Guid.NewGuid();
        var ct = TestContext.Current.CancellationToken;
        var options = CacheOptions.WithSlidingExpiration(Duration.FromMinutes(1));

        var gate1 = new TaskCompletionSource<Page>(TaskCreationOptions.RunContinuationsAsynchronously);
        ValueTask<Page> ThrowingFactory(Guid k, CancellationToken token) => new(gate1.Task);

        // act: start the throwing factory (entry1, in-flight), then remove the key and create a replacement
        var call1 = cache.GetOrCreateAsync(key, ThrowingFactory, options, ct).AsTask();
        await cache.RemoveAsync(key, ct);
        var replacement = await cache.GetOrCreateAsync(key, GetPageAsync, options, ct);

        // release factory1: it throws AFTER entry2 occupied the slot → its catch-path eviction must skip entry2
        gate1.TrySetException(new InvalidOperationException("boom"));
        // call1 is a local task created above via .AsTask(); awaiting it here is safe (not a foreign task)
#pragma warning disable VSTHRD003
        await Wrap.It(async () => await call1).ThrowsAsync<InvalidOperationException>();
#pragma warning restore VSTHRD003

        // assert: entry2 survived → a subsequent call returns the SAME cached instance without a new factory
        var subsequent = await cache.GetOrCreateAsync(key, GetPageAsync, options, ct);
        ReferenceEquals(subsequent, replacement).IsTrue();
        _factoryCounter.Is(1);
    }

    /// <summary>
    /// Verifies RemoveAsync during an in-flight factory: the awaiting caller still receives the produced value,
    /// and because the entry was purged, a subsequent call re-invokes the factory.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    protected async Task RemoveAsync_WhileFactoryInFlight_CallerGetsValueThenReinvokes_Base()
    {
        // arrange
        Get<ITimeProviderSwitcher>().UseManagedTime();
        var cache = Get<ICache<Guid, Page>>();
        Get<ITimeManager>().SetNow(SystemClock.Instance.GetCurrentInstant());
        var key = Guid.NewGuid();
        var ct = TestContext.Current.CancellationToken;
        var options = CacheOptions.WithSlidingExpiration(Duration.FromMinutes(1));

        var gate = new TaskCompletionSource<Page>(TaskCreationOptions.RunContinuationsAsynchronously);
        ValueTask<Page> GatedFactory(Guid k, CancellationToken token) => new(gate.Task);

        // act: start the gated factory, remove the key while it is in-flight, then release the factory
        var call1 = cache.GetOrCreateAsync(key, GatedFactory, options, ct).AsTask();
        await cache.RemoveAsync(key, ct);
        gate.TrySetResult(new Page(key));
        var result1 = await call1.WaitAsync(TimeSpan.FromSeconds(5), ct);
        result1.Is(new Page(key));

        // assert: the entry was purged mid-flight → a subsequent call re-invokes the factory
        var result2 = await cache.GetOrCreateAsync(key, GetPageAsync, options, ct);
        result2.Is(new Page(key));
        _factoryCounter.Is(1);
    }

    /// <summary>
    /// Validates that the cached items meet expected criteria including factory call count, item count, and reference equality.
    /// </summary>
    /// <param name="counter">The expected number of times the factory method should have been called.</param>
    /// <param name="key">The cache key used for item creation.</param>
    /// <param name="count">The expected number of items returned.</param>
    /// <param name="items">The array of items to validate.</param>
    private void EnsureItems(int counter, Guid key, int count, Page[] items)
    {
        _factoryCounter.Is(counter);
        items.Has(count);
        items[0].Is(new Page(key));
        foreach (var item in items)
            ReferenceEquals(item, items[0]).IsTrue();
    }

    /// <summary>
    /// Factory method for creating Page instances in cache tests.
    /// </summary>
    /// <param name="id">The unique identifier for the page.</param>
    /// <param name="ct">Cancellation token (unused; factory is shared work and does not honor per-caller CT).</param>
    /// <returns>A ValueTask containing the created Page instance.</returns>
    private ValueTask<Page> GetPageAsync(Guid id, CancellationToken ct)
    {
        Interlocked.Increment(ref _factoryCounter);

        return ValueTask.FromResult(new Page(id));
    }

    /// <summary>
    /// A test data model representing a page with title and content.
    /// </summary>
    private sealed record Page
    {
        /// <summary>
        /// Gets the title of the page.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Gets the content of the page.
        /// </summary>
        public string Content { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Page"/> record.
        /// </summary>
        /// <param name="key">The unique identifier used to generate title and content.</param>
        public Page(Guid key)
        {
            Title = $"{key}:title";
            Content = $"{key}:content";
        }
    }
}
