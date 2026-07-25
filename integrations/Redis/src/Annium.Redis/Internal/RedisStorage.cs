using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NodaTime;
using StackExchange.Redis;

namespace Annium.Redis.Internal;

/// <summary>
/// Internal implementation of <see cref="IRedisStorage"/> backed by StackExchange.Redis.
/// </summary>
/// <remarks>
/// The <see cref="ConnectionMultiplexer"/> is constructed lazily on first method call via
/// <see cref="Lazy{T}"/> with <see cref="LazyThreadSafetyMode.ExecutionAndPublication"/> —
/// the constructor does not block on a Redis connect, and concurrent first-callers share
/// the same connection task. Sticky-fail: if the connection task faults, every subsequent
/// caller observes the same fault for the lifetime of this instance.
/// </remarks>
internal class RedisStorage : IRedisStorage, IAsyncDisposable
{
    /// <summary>
    /// The Redis configuration used to establish the connection.
    /// </summary>
    private readonly RedisConfiguration _config;

    /// <summary>
    /// Lazily-constructed shared connection task; the multiplexer is created on first access
    /// and reused for the lifetime of this instance.
    /// </summary>
    private readonly Lazy<Task<ConnectionMultiplexer>> _redisLazy;

    /// <summary>
    /// Disposal flag that ensures <see cref="DisposeAsync"/> is idempotent (0 = live, 1 = disposed).
    /// </summary>
    private int _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="RedisStorage"/> class.
    /// </summary>
    /// <param name="config">The Redis configuration.</param>
    public RedisStorage(RedisConfiguration config)
    {
        _config = config;
        // VSTHRD011: Lazy<Task<T>> deadlock risk doesn't apply — ConnectionMultiplexer.ConnectAsync
        // doesn't capture the constructing thread's SynchronizationContext.
#pragma warning disable VSTHRD011
        _redisLazy = new Lazy<Task<ConnectionMultiplexer>>(ConnectAsync, LazyThreadSafetyMode.ExecutionAndPublication);
#pragma warning restore VSTHRD011
    }

    /// <summary>
    /// Enumerates all keys across every connected Redis server whose names match the given glob pattern.
    /// An empty or whitespace <paramref name="pattern"/> matches every key.
    /// </summary>
    /// <param name="pattern">Glob pattern used to filter keys (e.g. <c>session:*</c>). Pass an empty or whitespace-only string to match all keys.</param>
    /// <param name="ct">Cancellation token that cancels the key-scan operation.</param>
    /// <returns>A read-only set of key names that matched the pattern across all servers.</returns>
    public async Task<IReadOnlyCollection<string>> GetKeysAsync(string pattern = "", CancellationToken ct = default)
    {
        var redis = await GetConnectedMultiplexerAsync(ct);
        var keyPattern = string.IsNullOrWhiteSpace(pattern) ? default : new RedisValue(pattern);
        var keys = new HashSet<string>();

        foreach (var server in redis.GetServers())
        {
            await foreach (var key in server.KeysAsync(pattern: keyPattern).WithCancellation(ct))
                keys.Add(key.ToString());
        }

        return keys;
    }

    /// <summary>
    /// Retrieves the string value stored at the specified key, or <see langword="null"/> if the key does not exist.
    /// </summary>
    /// <param name="key">The Redis key whose value is to be retrieved.</param>
    /// <param name="ct">Cancellation token that cancels the get operation.</param>
    /// <returns>The stored string value, or <see langword="null"/> if the key is absent.</returns>
    public async Task<string?> GetAsync(string key, CancellationToken ct = default)
    {
        var redis = await GetConnectedMultiplexerAsync(ct);
        var value = await redis.GetDatabase().StringGetAsync(key);

        return value.IsNull ? null : value.ToString();
    }

    /// <summary>
    /// Stores a string value at the specified key, optionally with a sliding expiry.
    /// A zero <paramref name="expires"/> duration stores the key without an expiry.
    /// </summary>
    /// <param name="key">The Redis key under which the value is stored.</param>
    /// <param name="value">The string value to store.</param>
    /// <param name="expires">Time-to-live for the key; pass <see cref="Duration.Zero"/> for no expiry.</param>
    /// <param name="ct">Cancellation token that cancels the set operation.</param>
    /// <returns><see langword="true"/> if the value was set successfully; otherwise <see langword="false"/>.</returns>
    public async Task<bool> SetAsync(
        string key,
        string value,
        Duration expires = default,
        CancellationToken ct = default
    )
    {
        var redis = await GetConnectedMultiplexerAsync(ct);
        var result = await redis
            .GetDatabase()
            .StringSetAsync(key, value, expires == Duration.Zero ? null : expires.ToTimeSpan(), When.Always);

        return result;
    }

    /// <summary>
    /// Removes the specified key from the Redis database.
    /// </summary>
    /// <param name="key">The Redis key to delete.</param>
    /// <param name="ct">Cancellation token that cancels the delete operation.</param>
    /// <returns><see langword="true"/> if the key existed and was deleted; <see langword="false"/> if the key was not found.</returns>
    public async Task<bool> DeleteAsync(string key, CancellationToken ct = default)
    {
        var redis = await GetConnectedMultiplexerAsync(ct);
        var result = await redis.GetDatabase().KeyDeleteAsync(key);

        return result;
    }

    /// <summary>
    /// Disposes the lazily-constructed Redis connection (if any). Idempotent.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> that completes when the underlying <see cref="ConnectionMultiplexer"/> has been disposed.</returns>
    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
            return;

        if (!_redisLazy.IsValueCreated)
            return;

        try
        {
            var redis = await GetMultiplexerAsync();
            await redis.DisposeAsync();
        }
        catch
        {
            // ConnectAsync faulted — there is no live multiplexer to dispose.
        }
    }

    // Centralized lazy access. VSTHRD011 doesn't apply: the inner ConnectionMultiplexer.ConnectAsync
    // doesn't capture the constructing thread's SynchronizationContext, so the deadlock pattern the
    // analyzer warns about is unreachable. VSTHRD003 (foreign-Task) is likewise a non-issue: the
    // returned Task is produced by this type's own AsyncLazy field, not awaited from outside.
    /// <summary>
    /// Returns the shared, lazily-connected <see cref="ConnectionMultiplexer"/> task.
    /// The multiplexer is created exactly once; concurrent callers await the same task.
    /// </summary>
    /// <returns>A <see cref="Task{TResult}"/> that resolves to the shared <see cref="ConnectionMultiplexer"/>.</returns>
#pragma warning disable VSTHRD011, VSTHRD003
    private Task<ConnectionMultiplexer> GetMultiplexerAsync() => _redisLazy.Value;
#pragma warning restore VSTHRD011, VSTHRD003

    /// <summary>
    /// Observes the supplied token at the lazy-connection gate, then returns the shared
    /// <see cref="ConnectionMultiplexer"/> — the connection wait itself is cancellable via
    /// <paramref name="ct"/>. Centralizes the connection-gate preamble shared by every public operation.
    /// </summary>
    /// <param name="ct">Cancellation token observed at the connection gate.</param>
    /// <returns>A <see cref="Task{TResult}"/> that resolves to the shared <see cref="ConnectionMultiplexer"/>.</returns>
    private async Task<ConnectionMultiplexer> GetConnectedMultiplexerAsync(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        return await GetMultiplexerAsync().WaitAsync(ct);
    }

    /// <summary>
    /// Opens a new <see cref="ConnectionMultiplexer"/> connection using the configuration supplied at construction time.
    /// This method is called at most once by <see cref="_redisLazy"/>; the resulting task is cached for reuse.
    /// </summary>
    /// <returns>A <see cref="Task{TResult}"/> that resolves to the newly created <see cref="ConnectionMultiplexer"/>.</returns>
    private Task<ConnectionMultiplexer> ConnectAsync() =>
        ConnectionMultiplexer.ConnectAsync(_config.GetConnectionString());
}
