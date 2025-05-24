using System.Threading.Tasks;
using Testcontainers.Redis;

namespace Annium.Redis.Tests;

public class Database
{
    public RedisConfiguration Config { get; } = new();

    private readonly RedisContainer _db = new RedisBuilder().Build();

    public async Task InitAsync()
    {
        await _db.StartAsync();
        Config.Hosts = [new RedisHost(_db.Hostname, _db.GetMappedPublicPort(RedisBuilder.RedisPort))];
    }
}
