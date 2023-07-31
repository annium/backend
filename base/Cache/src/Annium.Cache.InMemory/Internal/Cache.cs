using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Annium.Cache.Abstractions;
using Annium.Extensions.Execution;
using NodaTime;

namespace Annium.Cache.InMemory.Internal;

internal class Cache<TKey, TValue> : ICache<TKey, TValue>, IAsyncDisposable
    where TKey : IEquatable<TKey>
    where TValue : notnull
{
    private readonly ITimeProvider _timeProvider;
    private readonly Dictionary<TKey, Entry> _data = new();
    private readonly IBackgroundExecutor _executor = Executor.Background.Concurrent<Cache<TKey, TValue>>();

    public Cache(
        ITimeProvider timeProvider
    )
    {
        _timeProvider = timeProvider;
        _executor.Start();
    }

    public async ValueTask<TValue> GetOrCreateAsync<TContext>(TKey key, Func<TKey, TContext, ValueTask<TValue>> factory, TContext context, CacheOptions options)
        where TContext : notnull
    {
        return await GetOrCreateEntry(key, factory, context, options).Tcs.Task;
    }

    public ValueTask RemoveAsync(TKey key)
    {
        lock (_data)
            _data.Remove(key);

        return ValueTask.CompletedTask;
    }


    public async ValueTask DisposeAsync()
    {
        await _executor.DisposeAsync();
    }

    private Entry GetOrCreateEntry<TContext>(TKey key, Func<TKey, TContext, ValueTask<TValue>> factory, TContext context, CacheOptions options)
    {
        lock (_data)
        {
            var now = _timeProvider.Now;
            if (_data.TryGetValue(key, out var entry) && entry.ExpiresAt > now)
                return entry.WithExpiresAt(options.GetExpiresAt(now));

            Console.WriteLine($"Create item for {key}");

            var tcs = new TaskCompletionSource<TValue>();
            var expiresAt = options.GetExpiresAt(now);

            _executor.Schedule(async () =>
            {
                Console.WriteLine($"Get {key} value");
                var value = await factory(key, context);

                if (expiresAt > _timeProvider.Now)
                    tcs.SetResult(value);
                else
                    lock (_data)
                        _data.Remove(key);
            });

            return _data[key] = new(tcs, expiresAt);
        }
    }

    private sealed record Entry(TaskCompletionSource<TValue> Tcs, Instant ExpiresAt)
    {
        public Instant ExpiresAt { get; private set; } = ExpiresAt;

        public Entry WithExpiresAt(Instant expiresAt)
        {
            ExpiresAt = expiresAt;

            return this;
        }
    }
}