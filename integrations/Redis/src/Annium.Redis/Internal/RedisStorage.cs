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
    private readonly RedisConfiguration _config;

    private readonly Lazy<Task<ConnectionMultiplexer>> _redisLazy;
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

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<string>> GetKeysAsync(string pattern = "", CancellationToken ct = default)
    {
        var redis = await GetMultiplexerAsync().WaitAsync(ct);
        var keyPattern = string.IsNullOrWhiteSpace(pattern) ? default : new RedisValue(pattern);
        var keys = new HashSet<string>();

        foreach (var server in redis.GetServers())
        {
            await foreach (var key in server.KeysAsync(pattern: keyPattern).WithCancellation(ct))
                keys.Add(key.ToString());
        }

        return keys;
    }

    /// <inheritdoc />
    public async Task<string?> GetAsync(string key, CancellationToken ct = default)
    {
        var redis = await GetMultiplexerAsync().WaitAsync(ct);
        var value = await redis.GetDatabase().StringGetAsync(key);

        return value.IsNull ? null : value.ToString();
    }

    /// <inheritdoc />
    public async Task<bool> SetAsync(
        string key,
        string value,
        Duration expires = default,
        CancellationToken ct = default
    )
    {
        var redis = await GetMultiplexerAsync().WaitAsync(ct);
        var result = await redis
            .GetDatabase()
            .StringSetAsync(key, value, expires == Duration.Zero ? null : expires.ToTimeSpan(), When.Always);

        return result;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(string key, CancellationToken ct = default)
    {
        var redis = await GetMultiplexerAsync().WaitAsync(ct);
        var result = await redis.GetDatabase().KeyDeleteAsync(key);

        return result;
    }

    /// <summary>
    /// Disposes the lazily-constructed Redis connection (if any). Idempotent.
    /// </summary>
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
#pragma warning disable VSTHRD011, VSTHRD003
    private Task<ConnectionMultiplexer> GetMultiplexerAsync() => _redisLazy.Value;
#pragma warning restore VSTHRD011, VSTHRD003

    private Task<ConnectionMultiplexer> ConnectAsync() =>
        ConnectionMultiplexer.ConnectAsync(_config.GetConnectionString());
}
