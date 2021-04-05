using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Annium.Core.Primitives
{
    public sealed class AsyncDisposableBox : DisposableBoxBase<AsyncDisposableBox>, IAsyncDisposable
    {
        private readonly List<IAsyncDisposable> _asyncDisposables = new();
        private readonly List<Func<Task>> _asyncDisposes = new();

        internal AsyncDisposableBox()
        {
        }

        public async ValueTask DisposeAsync()
        {
            DisposeBase();
            if (_asyncDisposables.Count > 0)
                await Task.WhenAll(Pull(_asyncDisposables).Select(async x => await x.DisposeAsync()));
            if (_asyncDisposes.Count > 0)
                await Task.WhenAll(Pull(_asyncDisposes).Select(async x => await x()));
        }

        public static AsyncDisposableBox operator +(AsyncDisposableBox box, IDisposable disposable) => box.Add(box.SyncDisposables, disposable);
        public static AsyncDisposableBox operator -(AsyncDisposableBox box, IDisposable disposable) => box.Remove(box.SyncDisposables, disposable);
        public static AsyncDisposableBox operator +(AsyncDisposableBox box, IEnumerable<IDisposable> disposables) => box.Add(box.SyncDisposables, disposables);
        public static AsyncDisposableBox operator -(AsyncDisposableBox box, IEnumerable<IDisposable> disposables) => box.Remove(box.SyncDisposables, disposables);
        public static AsyncDisposableBox operator +(AsyncDisposableBox box, IAsyncDisposable disposable) => box.Add(box._asyncDisposables, disposable);
        public static AsyncDisposableBox operator -(AsyncDisposableBox box, IAsyncDisposable disposable) => box.Remove(box._asyncDisposables, disposable);
        public static AsyncDisposableBox operator +(AsyncDisposableBox box, IEnumerable<IAsyncDisposable> disposables) => box.Add(box._asyncDisposables, disposables);
        public static AsyncDisposableBox operator -(AsyncDisposableBox box, IEnumerable<IAsyncDisposable> disposables) => box.Remove(box._asyncDisposables, disposables);
        public static AsyncDisposableBox operator +(AsyncDisposableBox box, Action dispose) => box.Add(box.SyncDisposes, dispose);
        public static AsyncDisposableBox operator -(AsyncDisposableBox box, Action dispose) => box.Remove(box.SyncDisposes, dispose);
        public static AsyncDisposableBox operator +(AsyncDisposableBox box, IEnumerable<Action> disposes) => box.Add(box.SyncDisposes, disposes);
        public static AsyncDisposableBox operator -(AsyncDisposableBox box, IEnumerable<Action> disposes) => box.Remove(box.SyncDisposes, disposes);
        public static AsyncDisposableBox operator +(AsyncDisposableBox box, Func<Task> dispose) => box.Add(box._asyncDisposes, dispose);
        public static AsyncDisposableBox operator -(AsyncDisposableBox box, Func<Task> dispose) => box.Remove(box._asyncDisposes, dispose);
        public static AsyncDisposableBox operator +(AsyncDisposableBox box, IEnumerable<Func<Task>> disposes) => box.Add(box._asyncDisposes, disposes);
        public static AsyncDisposableBox operator -(AsyncDisposableBox box, IEnumerable<Func<Task>> disposes) => box.Remove(box._asyncDisposes, disposes);
    }

    public sealed class DisposableBox : DisposableBoxBase<DisposableBox>, IDisposable
    {
        internal DisposableBox()
        {
        }

        public void Dispose()
        {
            DisposeBase();
        }

        public static DisposableBox operator +(DisposableBox box, IDisposable disposable) => box.Add(box.SyncDisposables, disposable);
        public static DisposableBox operator -(DisposableBox box, IDisposable disposable) => box.Remove(box.SyncDisposables, disposable);
        public static DisposableBox operator +(DisposableBox box, IEnumerable<IDisposable> disposables) => box.Add(box.SyncDisposables, disposables);
        public static DisposableBox operator -(DisposableBox box, IEnumerable<IDisposable> disposables) => box.Remove(box.SyncDisposables, disposables);
        public static DisposableBox operator +(DisposableBox box, Action dispose) => box.Add(box.SyncDisposes, dispose);
        public static DisposableBox operator -(DisposableBox box, Action dispose) => box.Remove(box.SyncDisposes, dispose);
        public static DisposableBox operator +(DisposableBox box, IEnumerable<Action> disposes) => box.Add(box.SyncDisposes, disposes);
        public static DisposableBox operator -(DisposableBox box, IEnumerable<Action> disposes) => box.Remove(box.SyncDisposes, disposes);
    }

    public abstract class DisposableBoxBase<TBox> where TBox : DisposableBoxBase<TBox>
    {
        public bool IsDisposed { get; private set; }
        protected readonly List<IDisposable> SyncDisposables = new();
        protected readonly List<Action> SyncDisposes = new();
        private readonly object _locker = new();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected TBox Add<T>(List<T> entries, T item)
        {
            EnsureNotDisposed();

            lock (_locker)
                entries.Add(item);

            return (TBox) this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected TBox Add<T>(List<T> entries, IEnumerable<T> items)
        {
            EnsureNotDisposed();

            lock (_locker)
                foreach (var item in items)
                    entries.Add(item);

            return (TBox) this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected TBox Remove<T>(List<T> entries, T item)
        {
            EnsureNotDisposed();

            lock (_locker)
                entries.Remove(item);

            return (TBox) this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected TBox Remove<T>(List<T> entries, IEnumerable<T> items)
        {
            EnsureNotDisposed();

            lock (_locker)
                foreach (var item in items)
                    entries.Remove(item);

            return (TBox) this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected IReadOnlyCollection<T> Pull<T>(List<T> entries)
        {
            var slice = entries.ToArray();
            entries.Clear();

            return slice;
        }

        protected void DisposeBase()
        {
            lock (_locker)
            {
                EnsureNotDisposed();
                IsDisposed = true;
            }

            if (SyncDisposables.Count > 0)
                foreach (var entry in Pull(SyncDisposables))
                    entry.Dispose();

            if (SyncDisposes.Count > 0)
                foreach (var entry in Pull(SyncDisposes))
                    entry();
        }

        private void EnsureNotDisposed()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(typeof(TBox).Name);
        }
    }
}