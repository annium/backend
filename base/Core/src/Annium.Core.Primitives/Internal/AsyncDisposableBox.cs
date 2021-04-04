using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Annium.Core.Primitives.Internal
{
    internal class AsyncDisposableBox : IAsyncDisposableBox
    {
        private readonly ConcurrentBag<IDisposable> _syncDisposables = new();
        private readonly ConcurrentBag<IAsyncDisposable> _asyncDisposables = new();
        private readonly ConcurrentBag<Action> _syncDisposes = new();
        private readonly ConcurrentBag<Func<Task>> _asyncDisposes = new();

        public IAsyncDisposableBox Add(IDisposable disposable) => Push(_syncDisposables, disposable);
        public IAsyncDisposableBox Add(IEnumerable<IDisposable> disposables) => PushRange(_syncDisposables, disposables);
        public IAsyncDisposableBox Add(IAsyncDisposable disposable) => Push(_asyncDisposables, disposable);
        public IAsyncDisposableBox Add(IEnumerable<IAsyncDisposable> disposables) => PushRange(_asyncDisposables, disposables);
        public IAsyncDisposableBox Add(Action dispose) => Push(_syncDisposes, dispose);
        public IAsyncDisposableBox Add(IEnumerable<Action> disposes) => PushRange(_syncDisposes, disposes);
        public IAsyncDisposableBox Add(Func<Task> dispose) => Push(_asyncDisposes, dispose);
        public IAsyncDisposableBox Add(IEnumerable<Func<Task>> disposes) => PushRange(_asyncDisposes, disposes);

        public async ValueTask DisposeAsync()
        {
            if (_syncDisposables.Count > 0)
                foreach (var entry in Pull(_syncDisposables))
                    entry.Dispose();

            if (_asyncDisposables.Count > 0)
                await Task.WhenAll(Pull(_asyncDisposables).Select(async x => await x.DisposeAsync()));

            if (_syncDisposes.Count > 0)
                foreach (var entry in Pull(_syncDisposes))
                    entry();

            if (_asyncDisposes.Count > 0)
                await Task.WhenAll(Pull(_asyncDisposes).Select(async x => await x()));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IAsyncDisposableBox Push<T>(ConcurrentBag<T> entries, T item)
        {
            entries.Add(item);

            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IAsyncDisposableBox PushRange<T>(ConcurrentBag<T> entries, IEnumerable<T> items)
        {
            foreach (var item in items)
                entries.Add(item);

            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IReadOnlyCollection<T> Pull<T>(ConcurrentBag<T> entries)
        {
            var slice = entries.ToArray();
            entries.Clear();

            return slice;
        }
    }
}