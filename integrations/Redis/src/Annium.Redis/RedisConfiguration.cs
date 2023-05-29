using System;

namespace Annium.Redis;

public record RedisConfiguration
{
    public RedisHost[] Hosts { get; set; } = Array.Empty<RedisHost>();
    public string User { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    public string ConnectionString => string.Join(
        ';',
        ""
    );
}

public sealed record RedisHost(string Host, int Port);