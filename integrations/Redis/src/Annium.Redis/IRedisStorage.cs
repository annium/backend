using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NodaTime;

namespace Annium.Redis;

/// <summary>
/// Provides Redis storage operations for string-based key-value pairs.
/// </summary>
/// <remarks>
/// Cancellation semantics: the supplied <see cref="CancellationToken"/> is honored at the
/// lazy-connection gate (the first method call waits for <see cref="StackExchange.Redis.ConnectionMultiplexer"/>
/// to connect) and, for <see cref="GetKeysAsync"/>, at the keyspace enumeration boundary.
/// In-flight individual database commands (StringGet/StringSet/KeyDelete) cannot be
/// cancelled mid-round-trip — StackExchange.Redis 2.x does not expose a
/// <see cref="CancellationToken"/> on those operations.
/// </remarks>
public interface IRedisStorage
{
    /// <summary>
    /// Retrieves all keys matching the specified pattern.
    /// </summary>
    /// <param name="pattern">The pattern to match keys against (an empty or whitespace-only string matches all keys).</param>
    /// <param name="ct">Cancellation token observed at the connection gate and during keyspace enumeration.</param>
    /// <returns>A collection of matching keys.</returns>
    Task<IReadOnlyCollection<string>> GetKeysAsync(string pattern = "", CancellationToken ct = default);

    /// <summary>
    /// Retrieves the value associated with the specified key.
    /// </summary>
    /// <param name="key">The key to retrieve.</param>
    /// <param name="ct">Cancellation token observed at the connection gate (not during the database round-trip).</param>
    /// <returns>The value if found, otherwise null.</returns>
    Task<string?> GetAsync(string key, CancellationToken ct = default);

    /// <summary>
    /// Sets a key-value pair with optional expiration.
    /// </summary>
    /// <param name="key">The key to set.</param>
    /// <param name="value">The value to set.</param>
    /// <param name="expires">Optional expiration duration (default means no expiration).</param>
    /// <param name="ct">Cancellation token observed at the connection gate (not during the database round-trip).</param>
    /// <returns>True if the operation succeeded, otherwise false.</returns>
    Task<bool> SetAsync(string key, string value, Duration expires = default, CancellationToken ct = default);

    /// <summary>
    /// Deletes the specified key and its associated value.
    /// </summary>
    /// <param name="key">The key to delete.</param>
    /// <param name="ct">Cancellation token observed at the connection gate (not during the database round-trip).</param>
    /// <returns>True if the key was deleted, false if it didn't exist.</returns>
    Task<bool> DeleteAsync(string key, CancellationToken ct = default);
}
