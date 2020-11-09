using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Annium.Core.Primitives.Internal
{
    internal class DisposableBox : IDisposableBox
    {
        private readonly ConcurrentBag<IDisposable> _syncDisposables = new ConcurrentBag<IDisposable>();
        private readonly ConcurrentBag<IAsyncDisposable> _asyncDisposables = new ConcurrentBag<IAsyncDisposable>();
        private readonly ConcurrentBag<Action> _syncDisposes = new ConcurrentBag<Action>();
        private readonly ConcurrentBag<Func<Task>> _asyncDisposes = new ConcurrentBag<Func<Task>>();

        public IDisposableBox Add(IDisposable disposable) => Push(_syncDisposables, disposable);

        public IDisposableBox Add(IAsyncDisposable disposable) => Push(_asyncDisposables, disposable);

        public IDisposableBox Add(Action dispose) => Push(_syncDisposes, dispose);

        public IDisposableBox Add(Func<Task> dispose) => Push(_asyncDisposes, dispose);

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
        private IDisposableBox Push<T>(ConcurrentBag<T> entries, T entry)
        {
            entries.Add(entry);

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