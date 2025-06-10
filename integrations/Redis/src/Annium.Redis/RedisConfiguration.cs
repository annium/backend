using System;
using System.Linq;
using System.Text;

namespace Annium.Redis;

/// <summary>
/// Configuration settings for Redis connection
/// </summary>
public record RedisConfiguration
{
    /// <summary>
    /// Gets or sets the Redis hosts to connect to
    /// </summary>
    public RedisHost[] Hosts { get; set; } = Array.Empty<RedisHost>();

    /// <summary>
    /// Gets or sets the username for authentication
    /// </summary>
    public string User { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the password for authentication
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Builds a connection string from the configuration settings
    /// </summary>
    /// <returns>A Redis connection string</returns>
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

/// <summary>
/// Represents a Redis host with hostname and port
/// </summary>
/// <param name="Host">The hostname or IP address</param>
/// <param name="Port">The port number</param>
public sealed record RedisHost(string Host, int Port)
{
    /// <summary>
    /// Returns the string representation of the Redis host in "host:port" format
    /// </summary>
    /// <returns>The host and port formatted as "host:port"</returns>
    public override string ToString() => $"{Host}:{Port}";
}
