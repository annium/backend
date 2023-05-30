using System.Threading;
using System.Threading.Tasks;
using Testcontainers.Redis;

namespace Annium.Redis.Tests;

public static class Database
{
    public static RedisConfiguration Config { get; } = new();

    private static readonly RedisContainer Db;
    private static readonly TaskCompletionSource InitTcs = new();
    private static volatile int _refs;

    static Database()
    {
        Db = new RedisBuilder().Build();
    }

    public static async Task AcquireAsync()
    {
        if (Interlocked.Increment(ref _refs) > 1)
        {
            await InitTcs.Task;
            return;
        }

        await Db.StartAsync();
        Config.Hosts = new[] { new RedisHost(Db.Hostname, Db.GetMappedPublicPort(RedisBuilder.RedisPort)) };
        InitTcs.SetResult();
    }
}