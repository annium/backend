using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using Annium;
using Annium.Extensions.Reactive.Internal;
using Annium.Logging;

// ReSharper disable once CheckNamespace
namespace System;

public static class BufferUntilSubscribedOperatorExtensions
{
    public static IObservable<T> BufferUntilSubscribed<T>(
        this IObservable<T> source,
        ILogger logger
    )
    {
        var ctx = new BufferContext<T>(source, logger);

        source.SubscribeOn(TaskPoolScheduler.Default).Subscribe(x => ctx.Process(x, null), e => ctx.Process(default!, e), ctx.DataCt);
        source.SubscribeOn(TaskPoolScheduler.Default).Subscribe(delegate { }, ctx.Complete, ctx.CompletionCt);

        ctx.Trace("create observable");

        return Observable.Create<T>(observer => ctx.IsFlushed ? ctx.Subscribe(observer) : ctx.SubscribeFirst(observer));
    }
}

file record BufferContext<T> : ILogSubject
{
    public ILogger Logger { get; }
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
        IObservable<T> source,
        ILogger logger
    )
    {
        Logger = logger;
        _source = source;
        _dataCts = CancellationTokenSource.CreateLinkedTokenSource(_completionCts.Token);
    }

    public IDisposable SubscribeFirst(IObserver<T> observer)
    {
        lock (this)
        {
            var target = observer.GetFullId();
            this.Trace<string>("{target} - start", target);

            Flush(observer);
            SetFirstObserver(observer);
            var subscription = Disposable.Create(DisposeFirstSubscription);

            this.Trace<string>("{target} - done", target);

            return subscription;
        }
    }

    public IDisposable Subscribe(IObserver<T> observer)
    {
        lock (this)
        {
            this.Trace("start");
            var subscription = _source.Subscribe(observer);
            this.Trace("done");

            return subscription;
        }
    }

    public void Process(T data, Exception? error)
    {
        lock (this)
        {
            this.Trace("start");

            if (_isCompleted)
                throw new InvalidOperationException($"Can't add: {data} - {error}, ctx already completed");

            var e = new ObservableEvent<T>(data, error, false);
            if (_firstObserver is null)
            {
                this.Trace("enqueue: {data} - {error}", data, error);
                _events.Enqueue(e);
            }
            else
            {
                this.Trace("first - handle: {data} - {error}", data, error);
                _firstObserver.Handle(e);
                SetFirstSubscription(Subscribe(_firstObserver));
            }

            this.Trace("done");
        }
    }

    public void Complete()
    {
        lock (this)
        {
            this.Trace("start");

            if (_isCompleted)
                throw new InvalidOperationException("source already completed");

            if (!IsFlushed)
            {
                this.Trace("enqueue: completed");
                _events.Enqueue(new ObservableEvent<T>(default!, null, true));
            }

            _isCompleted = true;

            _firstObserver = null;
            _completionCts.Cancel();

            this.Trace("done");
        }
    }

    private void Flush(IObserver<T> observer)
    {
        this.Trace("start");

        IsFlushed = true;
        while (_events.TryDequeue(out var e))
            observer.Handle(e);

        this.Trace("done");
    }

    private void SetFirstObserver(IObserver<T> observer)
    {
        this.Trace("start");

        if (_firstObserver is not null)
            throw new InvalidOperationException("first observer already subscribed");
        _firstObserver = observer;

        this.Trace("done");
    }

    private void SetFirstSubscription(IDisposable subscription)
    {
        this.Trace("start");

        if (!ReferenceEquals(_firstSubscription, Disposable.Empty))
            throw new InvalidOperationException("first subscription already set");

        _firstSubscription = subscription;
        _firstObserver = null;
        _dataCts.Cancel();

        this.Trace("done");
    }

    private void DisposeFirstSubscription()
    {
        this.Trace("start");

        _firstSubscription.Dispose();
        _firstSubscription = Disposable.Empty;

        this.Trace("done");
    }
}