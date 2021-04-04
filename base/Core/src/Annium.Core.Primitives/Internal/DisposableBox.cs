using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Annium.Core.Primitives.Internal
{
    internal class DisposableBox : IDisposableBox
    {
        private readonly ConcurrentBag<IDisposable> _syncDisposables = new();
        private readonly ConcurrentBag<Action> _syncDisposes = new();

        public IDisposableBox Add(IDisposable disposable) => Push(_syncDisposables, disposable);

        public IDisposableBox AddRange(IEnumerable<IDisposable> disposables) => PushRange(_syncDisposables, disposables);

        public IDisposableBox Add(Action dispose) => Push(_syncDisposes, dispose);

        public IDisposableBox Add(IEnumerable<Action> disposes) => PushRange(_syncDisposes, disposes);

        public void Dispose()
        {
            if (_syncDisposables.Count > 0)
                foreach (var entry in Pull(_syncDisposables))
                    entry.Dispose();

            if (_syncDisposes.Count > 0)
                foreach (var entry in Pull(_syncDisposes))
                    entry();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IDisposableBox Push<T>(ConcurrentBag<T> entries, T item)
        {
            entries.Add(item);

            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IDisposableBox PushRange<T>(ConcurrentBag<T> entries, IEnumerable<T> items)
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