using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Cache.Abstractions;
using Annium.Core.Runtime.Time;
using Annium.Testing;
using Annium.Testing.Collection;
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
        var count = 1000;

        // act
        var items = await Task.WhenAll(
            Enumerable.Range(0, count).Select(async _ => await cache.GetOrCreateAsync(key, GetPageAsync, options))
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

        var options1 = CacheOptions.WithAbsoluteExpiration(expiresAt);
        var count = 1000;

        // act
        var items = await Task.WhenAll(
            Enumerable.Range(0, count).Select(async _ => await cache.GetOrCreateAsync(key, GetPageAsync, options1))
        );

        // assert
        EnsureItems(1, key, count, items);

        // arrange
        timeManager.SetNow(expiresAt);
        expiresAt = timeProvider.Now + Duration.FromMinutes(1);
        var options2 = CacheOptions.WithAbsoluteExpiration(expiresAt);

        // act
        items = await Task.WhenAll(
            Enumerable.Range(0, count).Select(async _ => await cache.GetOrCreateAsync(key, GetPageAsync, options2))
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

        var options = CacheOptions.WithSlidingExpiration(lifetime);
        var count = 1000;

        // act
        var items = await Task.WhenAll(
            Enumerable.Range(0, count).Select(async _ => await cache.GetOrCreateAsync(key, GetPageAsync, options))
        );

        // assert
        EnsureItems(1, key, count, items);

        // arrange
        timeManager.SetNow(timeProvider.Now + lifetime);

        // act
        items = await Task.WhenAll(
            Enumerable.Range(0, count).Select(async _ => await cache.GetOrCreateAsync(key, GetPageAsync, options))
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

        var options = CacheOptions.WithSlidingExpiration(lifetime);
        var count = 1000;

        // act
        var items = await Task.WhenAll(
            Enumerable.Range(0, count).Select(async _ => await cache.GetOrCreateAsync(key, GetPageAsync, options))
        );

        // assert
        EnsureItems(1, key, count, items);

        // act
        await cache.RemoveAsync(key);

        // act
        items = await Task.WhenAll(
            Enumerable.Range(0, count).Select(async _ => await cache.GetOrCreateAsync(key, GetPageAsync, options))
        );

        // assert
        EnsureItems(2, key, count, items);
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
    /// <returns>A ValueTask containing the created Page instance.</returns>
    private ValueTask<Page> GetPageAsync(Guid id)
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
