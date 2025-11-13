using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NodaTime;
using StackExchange.Redis;

namespace Annium.Redis.Internal;

/// <summary>
/// Internal implementation of Redis storage using StackExchange.Redis
/// </summary>
internal class RedisStorage : IRedisStorage, IAsyncDisposable
{
    /// <summary>
    /// Gets the task representing the connection state
    /// </summary>
    private Task ConnectionTask => _connectionTcs.Task;

    /// <summary>
    /// Task completion source for tracking connection state
    /// </summary>
    private TaskCompletionSource _connectionTcs = new();

    /// <summary>
    /// The Redis connection multiplexer
    /// </summary>
    private readonly ConnectionMultiplexer _redis;

    /// <summary>
    /// Initializes a new instance of the RedisStorage class
    /// </summary>
    /// <param name="config">The Redis configuration</param>
    public RedisStorage(RedisConfiguration config)
    {
        _redis = ConnectionMultiplexer.Connect(config.GetConnectionString());

        _redis.ConnectionFailed += HandleConnectionFailed;
        _redis.ConnectionRestored += HandleConnectionRestored;

        if (_redis.IsConnected)
            _connectionTcs.SetResult();
    }

    /// <summary>
    /// Retrieves all keys matching the specified pattern from all Redis servers
    /// </summary>
    /// <param name="pattern">The pattern to match keys against</param>
    /// <returns>A collection of matching keys</returns>
    public async Task<IReadOnlyCollection<string>> GetKeysAsync(string pattern = "")
    {
#pragma warning disable VSTHRD003
        await ConnectionTask;
#pragma warning restore VSTHRD003

        var keyPattern = string.IsNullOrWhiteSpace(pattern) ? default : new RedisValue(pattern);
        var keys = new HashSet<string>();

        foreach (var server in _redis.GetServers())
        await foreach (var key in server.KeysAsync(pattern: keyPattern))
            keys.Add(key.ToString());

        return keys;
    }

    /// <summary>
    /// Retrieves the value associated with the specified key
    /// </summary>
    /// <param name="key">The key to retrieve</param>
    /// <returns>The value if found, otherwise null</returns>
    public async Task<string?> GetAsync(string key)
    {
#pragma warning disable VSTHRD003
        await ConnectionTask;
#pragma warning restore VSTHRD003

        var value = await _redis.GetDatabase().StringGetAsync(key);

        return value.IsNull ? null : value.ToString();
    }

    /// <summary>
    /// Sets a key-value pair with optional expiration
    /// </summary>
    /// <param name="key">The key to set</param>
    /// <param name="value">The value to set</param>
    /// <param name="expires">Optional expiration duration</param>
    /// <returns>True if the operation succeeded</returns>
    public async Task<bool> SetAsync(string key, string value, Duration expires = default)
    {
#pragma warning disable VSTHRD003
        await ConnectionTask;
#pragma warning restore VSTHRD003

        var result = await _redis
            .GetDatabase()
            .StringSetAsync(key, value, expires == Duration.Zero ? null : expires.ToTimeSpan(), When.Always);

        return result;
    }

    /// <summary>
    /// Deletes the specified key and its associated value
    /// </summary>
    /// <param name="key">The key to delete</param>
    /// <returns>True if the key was deleted, false if it didn't exist</returns>
    public async Task<bool> DeleteAsync(string key)
    {
#pragma warning disable VSTHRD003
        await ConnectionTask;
#pragma warning restore VSTHRD003

        var result = await _redis.GetDatabase().KeyDeleteAsync(key);

        return result;
    }

    /// <summary>
    /// Handles Redis connection failure events by resetting the connection task completion source
    /// </summary>
    /// <param name="sender">The event sender</param>
    /// <param name="args">Connection failure event arguments</param>
    private void HandleConnectionFailed(object? sender, ConnectionFailedEventArgs args) =>
        _connectionTcs = new TaskCompletionSource();

    /// <summary>
    /// Handles Redis connection restoration events by signaling successful connection
    /// </summary>
    /// <param name="sender">The event sender</param>
    /// <param name="args">Connection failure event arguments</param>
    private void HandleConnectionRestored(object? sender, ConnectionFailedEventArgs args) =>
        _connectionTcs.TrySetResult();

    /// <summary>
    /// Disposes the Redis connection and cleans up resources
    /// </summary>
    /// <returns>A ValueTask representing the asynchronous dispose operation</returns>
    public async ValueTask DisposeAsync()
    {
        _redis.ConnectionFailed -= HandleConnectionFailed;
        _redis.ConnectionRestored -= HandleConnectionRestored;

        await _redis.DisposeAsync();
    }
}
