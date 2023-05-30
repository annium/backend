using System;
using System.Linq;
using System.Text;

namespace Annium.Redis;

public record RedisConfiguration
{
    public RedisHost[] Hosts { get; set; } = Array.Empty<RedisHost>();
    public string User { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    public string GetConnectionString()
    {
        var sb = new StringBuilder();
        sb.AppendJoin(',', Hosts.Select(x => x.ToString()));

        if (!string.IsNullOrWhiteSpace(User))
            sb.Append($"user={User}");

        if (!string.IsNullOrWhiteSpace(Password))
            sb.Append($"password={Password}");

        return sb.ToString();
    }
}

public sealed record RedisHost(string Host, int Port)
{
    public override string ToString() => $"{Host}:{Port}";
}