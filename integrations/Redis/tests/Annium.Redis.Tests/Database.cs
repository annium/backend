using System.Threading;
using System.Threading.Tasks;
using Testcontainers.Redis;

namespace Annium.Redis.Tests;

public static class Database
{
    public static RedisConfiguration Config { get; } = new();

    private static readonly RedisContainer _db;
    private static readonly TaskCompletionSource _initTcs = new();
    private static volatile int _refs;

    static Database()
    {
        _db = new RedisBuilder().Build();
    }

    public static async Task AcquireAsync()
    {
        if (Interlocked.Increment(ref _refs) > 1)
        {
            await _initTcs.Task;
            return;
        }

        await _db.StartAsync();
        Config.Hosts = new[] { new RedisHost(_db.Hostname, _db.GetMappedPublicPort(RedisBuilder.RedisPort)) };
        _initTcs.SetResult();
    }
}
