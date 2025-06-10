using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Annium.Testing;
using Annium.Testing.Collection;
using NodaTime;
using Xunit;

namespace Annium.Redis.Tests;

public class RedisStorageTests : TestBase
{
    public RedisStorageTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        AddServicePack<ServicePack>();
    }

    [Fact]
    public async Task Set()
    {
        // arrange
        var storage = Get<IRedisStorage>();
        var key = Guid.NewGuid().ToString();
        var value = Guid.NewGuid().ToString();

        // ensure no data
        await EnsureDataIsEmpty(storage, key);

        // set key without ttl
        var result = await storage.SetAsync(key, value);
        result.IsTrue();

        // ensure data is present
        await EnsureDataIsPresent(storage, key, value);
    }

    [Fact]
    public async Task SetWithTtl()
    {
        // arrange
        var storage = Get<IRedisStorage>();
        var key = Guid.NewGuid().ToString();
        var value = Guid.NewGuid().ToString();
        var ttl = Duration.FromMilliseconds(100);

        // ensure no data
        await EnsureDataIsEmpty(storage, key);

        // set key with ttl
        var result = await storage.SetAsync(key, value, ttl);
        result.IsTrue();

        // ensure data is present
        await EnsureDataIsPresent(storage, key, value);

        // wait until expiration
        await Task.Delay((ttl + Duration.FromMilliseconds(1)).ToTimeSpan(), TestContext.Current.CancellationToken);

        // ensure no data
        await EnsureDataIsEmpty(storage, key);
    }

    [Fact]
    public async Task Delete()
    {
        // arrange
        var storage = Get<IRedisStorage>();
        var key = Guid.NewGuid().ToString();
        var value = Guid.NewGuid().ToString();

        // ensure no data
        await EnsureDataIsEmpty(storage, key);

        // set key
        var result = await storage.SetAsync(key, value);
        result.IsTrue();

        // ensure data is present
        await EnsureDataIsPresent(storage, key, value);

        // delete key
        result = await storage.DeleteAsync(key);
        result.IsTrue();

        // ensure no data
        await EnsureDataIsEmpty(storage, key);
    }

    private async Task EnsureDataIsEmpty(IRedisStorage storage, string key)
    {
        var (dbKeys, dbValue) = await LoadData(storage, key);

        // ensure data missing
        dbKeys.IsEmpty();
        dbValue.IsDefault();
    }

    private async Task EnsureDataIsPresent(IRedisStorage storage, string key, string value)
    {
        var (dbKeys, dbValue) = await LoadData(storage, key);

        // ensure data is present
        dbKeys.Has(1);
        dbValue.Is(value);
    }

    private async Task<(IReadOnlyCollection<string> keys, string? value)> LoadData(IRedisStorage storage, string key)
    {
        var pattern = $"*{key[2..10]}*";

        // find keys and try get value
        var keys = await storage.GetKeysAsync(pattern);
        var value = await storage.GetAsync(key);

        return (keys, value);
    }
}
