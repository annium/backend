using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Annium.Testing;
using Annium.Testing.Collection;
using NodaTime;
using Xunit;

namespace Annium.Redis.Tests;

/// <summary>
/// Tests for Redis storage operations including set, get, delete, and TTL functionality
/// </summary>
public class RedisStorageTests : TestBase
{
    /// <summary>
    /// Initializes a new instance of the RedisStorageTests class
    /// </summary>
    /// <param name="outputHelper">Test output helper for logging</param>
    public RedisStorageTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        AddServicePack<ServicePack>();
    }

    /// <summary>
    /// Tests basic key-value set operation without expiration
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
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

    /// <summary>
    /// Tests key-value set operation with time-to-live expiration
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
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

    /// <summary>
    /// Tests key deletion operation
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
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

    /// <summary>
    /// Ensures that no data exists for the specified key
    /// </summary>
    /// <param name="storage">Redis storage instance</param>
    /// <param name="key">Key to check</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    private async Task EnsureDataIsEmpty(IRedisStorage storage, string key)
    {
        var (dbKeys, dbValue) = await LoadData(storage, key);

        // ensure data missing
        dbKeys.IsEmpty();
        dbValue.IsDefault();
    }

    /// <summary>
    /// Ensures that the specified key-value pair exists in storage
    /// </summary>
    /// <param name="storage">Redis storage instance</param>
    /// <param name="key">Key to check</param>
    /// <param name="value">Expected value</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    private async Task EnsureDataIsPresent(IRedisStorage storage, string key, string value)
    {
        var (dbKeys, dbValue) = await LoadData(storage, key);

        // ensure data is present
        dbKeys.Has(1);
        dbValue.Is(value);
    }

    /// <summary>
    /// Loads data from storage for verification purposes
    /// </summary>
    /// <param name="storage">Redis storage instance</param>
    /// <param name="key">Key to load data for</param>
    /// <returns>Tuple containing matching keys and the value for the specified key</returns>
    private async Task<(IReadOnlyCollection<string> keys, string? value)> LoadData(IRedisStorage storage, string key)
    {
        var pattern = $"*{key[2..10]}*";

        // find keys and try get value
        var keys = await storage.GetKeysAsync(pattern);
        var value = await storage.GetAsync(key);

        return (keys, value);
    }
}
