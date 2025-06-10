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

public class CacheTestsBase : TestBase
{
    private int _factoryCounter;

    protected CacheTestsBase(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

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

    private void EnsureItems(int counter, Guid key, int count, Page[] items)
    {
        _factoryCounter.Is(counter);
        items.Has(count);
        items[0].Is(new Page(key));
        foreach (var item in items)
            ReferenceEquals(item, items[0]).IsTrue();
    }

    private ValueTask<Page> GetPageAsync(Guid id)
    {
        Interlocked.Increment(ref _factoryCounter);

        return ValueTask.FromResult(new Page(id));
    }

    private sealed record Page
    {
        public string Title { get; }
        public string Content { get; }

        public Page(Guid key)
        {
            Title = $"{key}:title";
            Content = $"{key}:content";
        }
    }
}
