using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NodaTime;
using StackExchange.Redis;

namespace Annium.Redis.Internal;

internal class RedisStorage : IRedisStorage, IAsyncDisposable
{
    private Task ConnectionTask => _connectionTcs.Task;
    private TaskCompletionSource _connectionTcs = new();
    private readonly ConnectionMultiplexer _redis;

    public RedisStorage(RedisConfiguration config)
    {
        _redis = ConnectionMultiplexer.Connect(config.GetConnectionString());

        _redis.ConnectionFailed += HandleConnectionFailed;
        _redis.ConnectionRestored += HandleConnectionRestored;

        if (_redis.IsConnected)
            _connectionTcs.SetResult();
    }

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

    public async Task<string?> GetAsync(string key)
    {
#pragma warning disable VSTHRD003
        await ConnectionTask;
#pragma warning restore VSTHRD003

        var value = await _redis.GetDatabase().StringGetAsync(key);

        return value.IsNull ? null : value.ToString();
    }

    public async Task<bool> SetAsync(string key, string value, Duration expires = default)
    {
#pragma warning disable VSTHRD003
        await ConnectionTask;
#pragma warning restore VSTHRD003

        var result = await _redis
            .GetDatabase()
            .StringSetAsync(key, value, expires == Duration.Zero ? null : expires.ToTimeSpan());

        return result;
    }

    public async Task<bool> DeleteAsync(string key)
    {
#pragma warning disable VSTHRD003
        await ConnectionTask;
#pragma warning restore VSTHRD003

        var result = await _redis.GetDatabase().KeyDeleteAsync(key);

        return result;
    }

    private void HandleConnectionFailed(object? sender, ConnectionFailedEventArgs args) =>
        _connectionTcs = new TaskCompletionSource();

    private void HandleConnectionRestored(object? sender, ConnectionFailedEventArgs args) =>
        _connectionTcs.TrySetResult();

    public async ValueTask DisposeAsync()
    {
        _redis.ConnectionFailed -= HandleConnectionFailed;
        _redis.ConnectionRestored -= HandleConnectionRestored;

        await _redis.DisposeAsync();
    }
}
