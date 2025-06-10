using System.Collections.Generic;
using System.Threading.Tasks;
using NodaTime;

namespace Annium.Redis;

/// <summary>
/// Provides Redis storage operations for string-based key-value pairs
/// </summary>
public interface IRedisStorage
{
    /// <summary>
    /// Retrieves all keys matching the specified pattern
    /// </summary>
    /// <param name="pattern">The pattern to match keys against (empty string matches all keys)</param>
    /// <returns>A collection of matching keys</returns>
    Task<IReadOnlyCollection<string>> GetKeysAsync(string pattern = "");

    /// <summary>
    /// Retrieves the value associated with the specified key
    /// </summary>
    /// <param name="key">The key to retrieve</param>
    /// <returns>The value if found, otherwise null</returns>
    Task<string?> GetAsync(string key);

    /// <summary>
    /// Sets a key-value pair with optional expiration
    /// </summary>
    /// <param name="key">The key to set</param>
    /// <param name="value">The value to set</param>
    /// <param name="expires">Optional expiration duration (default means no expiration)</param>
    /// <returns>True if the operation succeeded, otherwise false</returns>
    Task<bool> SetAsync(string key, string value, Duration expires = default);

    /// <summary>
    /// Deletes the specified key and its associated value
    /// </summary>
    /// <param name="key">The key to delete</param>
    /// <returns>True if the key was deleted, false if it didn't exist</returns>
    Task<bool> DeleteAsync(string key);
}
