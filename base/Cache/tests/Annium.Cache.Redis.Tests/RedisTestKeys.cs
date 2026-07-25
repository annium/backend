using System;

namespace Annium.Cache.Redis.Tests;

/// <summary>
/// Helpers for reconstructing the storage keys the Redis cache derives, for white-box assertions that peek at
/// the backing store directly.
/// </summary>
internal static class RedisTestKeys
{
    /// <summary>
    /// Reconstructs the storage key the cache derives for a <c>"test:"</c>-prefixed <c>ICache&lt;Guid,TValue&gt;</c>:
    /// the configured prefix + the value-type discriminator + the key. Mirrors <c>Cache.Key()</c> so a test can
    /// assert the exact stored key without hard-coding the formula at each call site.
    /// </summary>
    /// <typeparam name="TValue">The cache value type whose discriminator is folded into the key.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <returns>The prefixed, value-type-scoped storage key.</returns>
    public static string Prefixed<TValue>(Guid key) => $"test:{typeof(TValue).FullName}:{key}";
}
