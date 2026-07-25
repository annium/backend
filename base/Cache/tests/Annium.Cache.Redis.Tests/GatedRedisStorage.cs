using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Redis;
using NodaTime;

namespace Annium.Cache.Redis.Tests;

/// <summary>
/// In-memory <see cref="IRedisStorage"/> test double with a one-shot gate on <see cref="SetAsync"/>, used to
/// deterministically interleave a concurrent <c>RemoveAsync</c> with an in-flight cache write. Physical expiry
/// (<c>expires</c>) is ignored — the cache under test drives expiry logically via <c>ITimeProvider</c>.
/// </summary>
internal sealed class GatedRedisStorage : IRedisStorage
{
    /// <summary>
    /// In-memory backing store (key → serialized value). Physical TTL is not modelled.
    /// </summary>
    private readonly ConcurrentDictionary<string, string> _data = new();

    /// <summary>
    /// One-shot gate signal, completed when a gated <see cref="SetAsync"/> enters and pauses; null when disarmed.
    /// </summary>
    private TaskCompletionSource? _setEntered;

    /// <summary>
    /// One-shot gate release, completed by <see cref="ReleaseSet"/> to let a paused <see cref="SetAsync"/> finish.
    /// </summary>
    private TaskCompletionSource? _setReleased;

    /// <summary>
    /// The <c>expires</c> argument of the most recent <see cref="SetAsync"/> call — lets a test assert the
    /// physical TTL the cache computed (e.g. the near-immediate-expiry clamp) instead of trusting a round-trip.
    /// </summary>
    public Duration LastSetExpires { get; private set; }

    /// <summary>
    /// Returns all stored keys (the <paramref name="pattern"/> is ignored — the cache does not enumerate keys).
    /// </summary>
    /// <param name="pattern">Ignored.</param>
    /// <param name="ct">Cancellation token (unused).</param>
    /// <returns>A snapshot of all stored keys.</returns>
    public Task<IReadOnlyCollection<string>> GetKeysAsync(string pattern = "", CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyCollection<string>>(_data.Keys.ToArray());

    /// <summary>
    /// Retrieves the stored value for a key.
    /// </summary>
    /// <param name="key">The key to read.</param>
    /// <param name="ct">Cancellation token (unused).</param>
    /// <returns>The value if present, otherwise null.</returns>
    public Task<string?> GetAsync(string key, CancellationToken ct = default) =>
        Task.FromResult(_data.TryGetValue(key, out var v) ? v : null);

    /// <summary>
    /// Stores a value for a key. When the gate is armed (see <see cref="ArmSetGate"/>), the call signals it and
    /// blocks until <see cref="ReleaseSet"/> before writing; <see cref="LastSetExpires"/> records the ttl.
    /// </summary>
    /// <param name="key">The key to set.</param>
    /// <param name="value">The value to store.</param>
    /// <param name="expires">The requested physical TTL (recorded into <see cref="LastSetExpires"/>, not enforced).</param>
    /// <param name="ct">Cancellation token (unused).</param>
    /// <returns>Always true.</returns>
    public async Task<bool> SetAsync(
        string key,
        string value,
        Duration expires = default,
        CancellationToken ct = default
    )
    {
        LastSetExpires = expires;

        var entered = _setEntered;
        if (entered is not null)
        {
            // one-shot: disarm entry so the released write (and any later write) is not re-gated; keep
            // _setReleased non-null so ReleaseSet can still signal the captured completion source below.
            var released = _setReleased!;
            _setEntered = null;
            entered.TrySetResult();
            await released.Task;
        }

        _data[key] = value;
        return true;
    }

    /// <summary>
    /// Deletes a key.
    /// </summary>
    /// <param name="key">The key to delete.</param>
    /// <param name="ct">Cancellation token (unused).</param>
    /// <returns>True if the key existed and was removed, otherwise false.</returns>
    public Task<bool> DeleteAsync(string key, CancellationToken ct = default) =>
        Task.FromResult(_data.TryRemove(key, out _));

    /// <summary>
    /// Arms a one-shot gate: the next <see cref="SetAsync"/> signals the returned task, then blocks until
    /// <see cref="ReleaseSet"/> is called.
    /// </summary>
    /// <returns>A task that completes when the next <see cref="SetAsync"/> has entered and paused.</returns>
    public Task ArmSetGate()
    {
        _setEntered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        _setReleased = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        return _setEntered.Task;
    }

    /// <summary>
    /// Releases the armed <see cref="SetAsync"/> so it completes its write.
    /// </summary>
    public void ReleaseSet() => _setReleased?.TrySetResult();

    /// <summary>
    /// Whether the backing store currently holds the given key.
    /// </summary>
    /// <param name="key">The (prefixed) storage key.</param>
    /// <returns><c>true</c> if present.</returns>
    public bool Has(string key) => _data.ContainsKey(key);
}
