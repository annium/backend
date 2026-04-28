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
        var count = 1000;

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
        var count = 1000;

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
        var count = 1000;

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
        var count = 1000;

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
        var ex = await Wrap
            .It(async () =>
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
        var count = 1000;

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
        cts.Cancel();

        // act + assert
        await Wrap
            .It(async () => await cache.GetOrCreateAsync(key, GetPageAsync, options, cts.Token))
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

        var factoryGate = new TaskCompletionSource<Page>(TaskCreationOptions.RunContinuationsAsynchronously);
        ValueTask<Page> SlowFactory(Guid k, CancellationToken token) => new(factoryGate.Task);

        using var cts1 = new CancellationTokenSource();

        var task1 = cache.GetOrCreateAsync(key, SlowFactory, options, cts1.Token).AsTask();
        var task2 = cache.GetOrCreateAsync(key, SlowFactory, options, ct).AsTask();

        // give both callers a moment to start awaiting the shared TCS
        await Task.Delay(100, ct);

        // act 1: cancel caller 1 — its await should throw, factory continues
        cts1.Cancel();
        await Wrap.It(async () => await task1).ThrowsAsync<OperationCanceledException>();

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
        cts.Cancel();

        // act + assert
        await Wrap.It(async () => await cache.RemoveAsync(key, cts.Token)).ThrowsAsync<OperationCanceledException>();
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
