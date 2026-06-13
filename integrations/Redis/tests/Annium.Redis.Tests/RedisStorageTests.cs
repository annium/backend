using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Annium.Testing;
using NodaTime;
using Xunit;

namespace Annium.Redis.Tests;

/// <summary>
/// Tests for Redis storage operations including set, get, delete, TTL, cancellation, and dispose semantics.
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
        RegisterServicePack<ServicePack>();
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
        var ct = TestContext.Current.CancellationToken;

        // ensure no data
        await EnsureDataIsEmpty(storage, key, ct);

        // set key without ttl
        var result = await storage.SetAsync(key, value, ct: ct);
        result.IsTrue();

        // ensure data is present
        await EnsureDataIsPresent(storage, key, value, ct);
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
        var ct = TestContext.Current.CancellationToken;

        // ensure no data
        await EnsureDataIsEmpty(storage, key, ct);

        // set key with ttl
        var result = await storage.SetAsync(key, value, ttl, ct);
        result.IsTrue();

        // ensure data is present
        await EnsureDataIsPresent(storage, key, value, ct);

        // wait until expiration
        await Task.Delay((ttl + Duration.FromMilliseconds(1)).ToTimeSpan(), ct);

        // ensure no data
        await EnsureDataIsEmpty(storage, key, ct);
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
        var ct = TestContext.Current.CancellationToken;

        // ensure no data
        await EnsureDataIsEmpty(storage, key, ct);

        // set key
        var result = await storage.SetAsync(key, value, ct: ct);
        result.IsTrue();

        // ensure data is present
        await EnsureDataIsPresent(storage, key, value, ct);

        // delete key
        result = await storage.DeleteAsync(key, ct);
        result.IsTrue();

        // ensure no data
        await EnsureDataIsEmpty(storage, key, ct);
    }

    /// <summary>
    /// Verifies SetAsync observes a pre-cancelled CT at the lazy-connection gate.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task SetAsync_PreCancelledCt_Throws()
    {
        var storage = Get<IRedisStorage>();
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Wrap.It(async () => await storage.SetAsync(Guid.NewGuid().ToString(), "v", ct: cts.Token))
            .ThrowsAsync<OperationCanceledException>();
    }

    /// <summary>
    /// Verifies GetAsync observes a pre-cancelled CT at the lazy-connection gate.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task GetAsync_PreCancelledCt_Throws()
    {
        var storage = Get<IRedisStorage>();
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Wrap.It(async () => await storage.GetAsync(Guid.NewGuid().ToString(), cts.Token))
            .ThrowsAsync<OperationCanceledException>();
    }

    /// <summary>
    /// Verifies DeleteAsync observes a pre-cancelled CT at the lazy-connection gate.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task DeleteAsync_PreCancelledCt_Throws()
    {
        var storage = Get<IRedisStorage>();
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Wrap.It(async () => await storage.DeleteAsync(Guid.NewGuid().ToString(), cts.Token))
            .ThrowsAsync<OperationCanceledException>();
    }

    /// <summary>
    /// Verifies GetKeysAsync observes a pre-cancelled CT at the lazy-connection gate.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task GetKeysAsync_PreCancelledCt_Throws()
    {
        var storage = Get<IRedisStorage>();
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Wrap.It(async () => await storage.GetKeysAsync(ct: cts.Token)).ThrowsAsync<OperationCanceledException>();
    }

    /// <summary>
    /// Verifies DisposeAsync is idempotent — second call is a no-op and does not throw.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task DisposeAsync_CalledTwice_DoesNotThrow()
    {
        var disposable = (IAsyncDisposable)Get<IRedisStorage>();

        await disposable.DisposeAsync();
        await disposable.DisposeAsync();
    }

    /// <summary>
    /// Verifies DisposeAsync without any prior method call does not throw and does not force a connection
    /// (relies on the IsValueCreated short-circuit on the lazy multiplexer).
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task DisposeAsync_WithoutFirstCall_DoesNotThrow()
    {
        var disposable = (IAsyncDisposable)Get<IRedisStorage>();

        await disposable.DisposeAsync();
    }

    /// <summary>
    /// Ensures that no data exists for the specified key
    /// </summary>
    /// <param name="storage">Redis storage instance</param>
    /// <param name="key">Key to check</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    private async Task EnsureDataIsEmpty(IRedisStorage storage, string key, CancellationToken ct)
    {
        var (dbKeys, dbValue) = await LoadData(storage, key, ct);

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
    /// <param name="ct">Cancellation token</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    private async Task EnsureDataIsPresent(IRedisStorage storage, string key, string value, CancellationToken ct)
    {
        var (dbKeys, dbValue) = await LoadData(storage, key, ct);

        // ensure data is present
        dbKeys.Has(1);
        dbValue.Is(value);
    }

    /// <summary>
    /// Loads data from storage for verification purposes
    /// </summary>
    /// <param name="storage">Redis storage instance</param>
    /// <param name="key">Key to load data for</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Tuple containing matching keys and the value for the specified key</returns>
    private async Task<(IReadOnlyCollection<string> keys, string? value)> LoadData(
        IRedisStorage storage,
        string key,
        CancellationToken ct
    )
    {
        var pattern = $"*{key[2..10]}*";

        // find keys and try get value
        var keys = await storage.GetKeysAsync(pattern, ct);
        var value = await storage.GetAsync(key, ct);

        return (keys, value);
    }
}
