using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Annium.Core.Primitives.Internal
{
    internal class DisposableBox : IDisposableBox
    {
        private readonly object _locker = new object();
        private readonly IList<IDisposable> _syncDisposables = new List<IDisposable>();
        private readonly IList<IAsyncDisposable> _asyncDisposables = new List<IAsyncDisposable>();
        private readonly IList<Action> _syncDisposes = new List<Action>();
        private readonly IList<Func<Task>> _asyncDisposes = new List<Func<Task>>();

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
        private IDisposableBox Push<T>(IList<T> entries, T entry)
        {
            lock (_locker)
                entries.Add(entry);

            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IReadOnlyCollection<T> Pull<T>(IList<T> entries)
        {
            lock (_locker)
            {
                var slice = entries.ToArray();
                entries.Clear();
                return slice;
            }
        }
    }
}