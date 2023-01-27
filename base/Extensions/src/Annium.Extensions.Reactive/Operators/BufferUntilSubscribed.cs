using System.Collections.Generic;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Annium.Core.Internal;
using Annium.Core.Primitives;
using Annium.Extensions.Reactive.Internal;

namespace System;

public static class BufferUntilSubscribedOperatorExtensions
{
    public static IObservable<T> BufferUntilSubscribed<T>(
        this IObservable<T> source
    )
    {
        var ctx = new BufferContext<T>(source);

        source.Subscribe(x => ctx.Process(x, null), e => ctx.Process(default!, e), ctx.DataCt);
        source.Subscribe(delegate { }, ctx.Complete, ctx.CompletionCt);

        ctx.Trace("create observable");

        return Observable.Create<T>(observer => ctx.IsFlushed ? ctx.Subscribe(observer) : ctx.SubscribeFirst(observer));
    }
}

file record BufferContext<T>
{
    public bool IsFlushed { get; private set; }
    public CancellationToken DataCt => _dataCts.Token;
    public CancellationToken CompletionCt => _completionCts.Token;
    private readonly IObservable<T> _source;
    private readonly Queue<ObservableEvent<T>> _events = new();
    private readonly CancellationTokenSource _dataCts;
    private readonly CancellationTokenSource _completionCts = new();
    private bool _isCompleted;
    private IObserver<T>? _firstObserver;
    private IDisposable _firstSubscription = Disposable.Empty;

    public BufferContext(
        IObservable<T> source
    )
    {
        _source = source;
        _dataCts = CancellationTokenSource.CreateLinkedTokenSource(_completionCts.Token);
    }

    public IDisposable SubscribeFirst(IObserver<T> observer)
    {
        lock (this)
        {
            var target = observer.GetFullId();
            Trace($"{target} - start");

            Flush(observer);
            SetFirstObserver(observer);
            var subscription = Disposable.Create(DisposeFirstSubscription);

            Trace($"{target} - done");

            return subscription;
        }
    }

    public IDisposable Subscribe(IObserver<T> observer)
    {
        lock (this)
        {
            Trace("start");
            var subscription = _source.Subscribe(observer);
            Trace("done");

            return subscription;
        }
    }

    public void Process(T data, Exception? error)
    {
        lock (this)
        {
            Trace("start");

            if (_isCompleted)
                throw new InvalidOperationException($"Can't add: {data} - {error}, ctx already completed");

            var e = new ObservableEvent<T>(data, error, false);
            if (_firstObserver is null)
            {
                Trace($"enqueue: {data} - {error}");
                _events.Enqueue(e);
            }
            else
            {
                Trace($"first - handle: {data} - {error}");
                _firstObserver.Handle(e);
                SetFirstSubscription(Subscribe(_firstObserver));
            }

            Trace("done");
        }
    }

    public void Complete()
    {
        lock (this)
        {
            Trace("start");

            if (_isCompleted)
                throw new InvalidOperationException("source already completed");

            if (!IsFlushed)
            {
                Trace("enqueue: completed");
                _events.Enqueue(new ObservableEvent<T>(default!, null, true));
            }

            _isCompleted = true;

            _firstObserver = null;
            _completionCts.Cancel();

            Trace("done");
        }
    }

    private void Flush(IObserver<T> observer)
    {
        Trace("start");

        IsFlushed = true;
        while (_events.TryDequeue(out var e))
            observer.Handle(e);

        Trace("done");
    }

    private void SetFirstObserver(IObserver<T> observer)
    {
        Trace("start");

        if (_firstObserver is not null)
            throw new InvalidOperationException("first observer already subscribed");
        _firstObserver = observer;

        Trace("done");
    }

    private void SetFirstSubscription(IDisposable subscription)
    {
        Trace("start");

        if (!ReferenceEquals(_firstSubscription, Disposable.Empty))
            throw new InvalidOperationException("first subscription already set");

        _firstSubscription = subscription;
        _firstObserver = null;
        _dataCts.Cancel();

        Trace("done");
    }

    private void DisposeFirstSubscription()
    {
        Trace("start");

        _firstSubscription.Dispose();
        _firstSubscription = Disposable.Empty;

        Trace("done");
    }

    public void Trace(
        string msg,
        [CallerFilePath] string callerFilePath = "",
        [CallerMemberName] string member = "",
        [CallerLineNumber] int line = 0
    )
    {
        this.Trace(msg, false, callerFilePath, member, line);
    }
}