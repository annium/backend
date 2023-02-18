using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Annium.Debug;

namespace Annium;

public sealed class AsyncDisposableBox : DisposableBoxBase<AsyncDisposableBox>, IAsyncDisposable
{
    private readonly List<IAsyncDisposable> _asyncDisposables = new();
    private readonly List<Func<Task>> _asyncDisposes = new();

    internal AsyncDisposableBox()
    {
    }

    public async ValueTask DisposeAsync()
    {
        this.Trace("start");

        DisposeBase();

        if (_asyncDisposables.Count > 0)
            await Task.WhenAll(Pull(_asyncDisposables).Select(async entry =>
            {
                this.Trace($"dispose {entry.GetFullId()} - start");
                await entry.DisposeAsync();
                this.Trace($"dispose {entry.GetFullId()} - done");
            }));
        if (_asyncDisposes.Count > 0)
            await Task.WhenAll(Pull(_asyncDisposes).Select(async entry =>
            {
                this.Trace($"dispose {entry.GetFullId()} - start");
                await entry();
                this.Trace($"dispose {entry.GetFullId()} - done");
            }));

        this.Trace("done");
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
        this.Trace("start");
        DisposeBase();
        this.Trace("done");
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

    public void EnsureNotDisposed()
    {
        if (IsDisposed)
            throw new ObjectDisposedException(typeof(TBox).Name);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected TBox Add<T>(List<T> entries, T item)
    {
        EnsureNotDisposed();

        lock (_locker)
        {
            this.Trace($"add {item.GetFullId()}");
            entries.Add(item);
        }

        return (TBox) this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected TBox Add<T>(List<T> entries, IEnumerable<T> items)
    {
        EnsureNotDisposed();

        lock (_locker)
            foreach (var item in items)
            {
                this.Trace($"add {item.GetFullId()}");
                entries.Add(item);
            }

        return (TBox) this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected TBox Remove<T>(List<T> entries, T item)
    {
        EnsureNotDisposed();

        lock (_locker)
        {
            this.Trace($"remove {item.GetFullId()}");
            entries.Remove(item);
        }

        return (TBox) this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected TBox Remove<T>(List<T> entries, IEnumerable<T> items)
    {
        EnsureNotDisposed();

        lock (_locker)
            foreach (var item in items)
            {
                this.Trace($"remove {item.GetFullId()}");
                entries.Remove(item);
            }

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
            if (IsDisposed)
            {
                this.Trace("already disposed");
                return;
            }

            IsDisposed = true;
        }

        if (SyncDisposables.Count > 0)
            foreach (var entry in Pull(SyncDisposables))
            {
                this.Trace($"dispose {entry.GetFullId()} - start");
                entry.Dispose();
                this.Trace($"dispose {entry.GetFullId()} - done");
            }

        if (SyncDisposes.Count > 0)
            foreach (var entry in Pull(SyncDisposes))
            {
                this.Trace($"dispose {entry.GetFullId()} - start");
                entry();
                this.Trace($"dispose {entry.GetFullId()} - done");
            }
    }
}