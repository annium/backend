using System.Collections.Generic;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Annium.Core.Primitives;
using Annium.Extensions.Reactive.Internal;

namespace System
{
    public static class BufferUntilSubscribedOperatorExtensions
    {
        public static IObservable<T> BufferUntilSubscribed<T>(
            this IObservable<T> source
        )
        {
            var ctx = new Context<T>(source);

            source.Subscribe(x => Add(ctx, x, null), e => Add(ctx, default!, e), ctx.DataCt);
            source.Subscribe(delegate { }, () => Complete(ctx), ctx.CompletionCt);

            // ctx.Trace("init buffering");

            return Observable.Create(CreateObservable(ctx));
        }

        private static Func<IObserver<T>, IDisposable> CreateObservable<T>(Context<T> ctx) => observer =>
        {
            lock (ctx)
            {
                // var target = $"{observer}#{observer.GetId()}";
                // ctx.Trace($"{target} - subscribe");

                if (ctx.IsFlushed)
                {
                    if (ctx.IsCompleted)
                    {
                        // ctx.Trace($"{target} - flushed, completed");

                        return Disposable.Empty;
                    }

                    // ctx.Trace($"{target} - flushed, attached");
                    return ctx.Subscribe(observer);
                }

                // ctx.Trace($"{target} - pipe to");

                while (ctx.Events.TryDequeue(out var e))
                    observer.Handle(e);

                ctx.Flush();

                if (ctx.IsCompleted)
                {
                    // ctx.Trace($"{target} - piped, completed");

                    return Disposable.Empty;
                }

                // ctx.Trace($"{target} - piped, attached");
                ctx.SetFirstObserver(observer);

                // ctx.Trace($"{target} - done");

                return Disposable.Create(ctx.DisposeFirstSubscription);
            }
        };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Add<T>(Context<T> ctx, T data, Exception? error)
        {
            lock (ctx)
            {
                // ctx.Trace("start");

                if (ctx.IsCompleted)
                    throw new InvalidOperationException($"Can't add: {data} - {error}, ctx already completed");

                var e = new ObservableEvent<T>(data, error, false);
                if (ctx.FirstObserver is null)
                {
                    // ctx.Trace($"enqueue: {data} - {error}");
                    ctx.Events.Enqueue(e);
                }
                else
                {
                    // ctx.Trace($"process: {data} - {error}");
                    ctx.FirstObserver.Handle(e);
                    if (!e.IsCompleted)
                        ctx.SetFirstSubscription(ctx.Subscribe(ctx.FirstObserver));
                }

                // ctx.Trace("done");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Complete<T>(Context<T> ctx)
        {
            lock (ctx)
            {
                // ctx.Trace("start");

                if (ctx.IsCompleted)
                    throw new InvalidOperationException("Can't complete, ctx already completed");

                if (!ctx.IsFlushed)
                {
                    // ctx.Trace("enqueue: completed");
                    ctx.Events.Enqueue(new ObservableEvent<T>(default!, null, true));
                }

                ctx.Complete();

                // ctx.Trace("done");
            }
        }

        private class Context<T>
        {
            public IObservable<T> Source { get; }
            public Queue<ObservableEvent<T>> Events { get; } = new();
            public bool IsFlushed { get; private set; }
            public bool IsCompleted { get; private set; }
            public IObserver<T>? FirstObserver { get; private set; }
            public IDisposable FirstSubscription { get; private set; } = Disposable.Empty;
            public CancellationToken DataCt => _dataCts.Token;
            public CancellationToken CompletionCt => _completionCts.Token;
            private readonly List<IObserver<T>> _incompleteObservers = new();
            private readonly CancellationTokenSource _dataCts;
            private readonly CancellationTokenSource _completionCts = new();

            public Context(
                IObservable<T> source
            )
            {
                Source = source;
                _dataCts = CancellationTokenSource.CreateLinkedTokenSource(_completionCts.Token);
            }

            public void Flush()
            {
                // this.Trace("start");

                if (IsFlushed)
                    throw new InvalidOperationException("source already flushed");
                IsFlushed = true;

                // this.Trace("done");
            }

            public void Complete()
            {
                // this.Trace("start");

                if (IsCompleted)
                    throw new InvalidOperationException("source already completed");
                IsCompleted = true;

                if (FirstObserver is not null)
                    FirstObserver = null;

                _completionCts.Cancel();

                // this.Trace("done");
            }

            public IDisposable Subscribe(IObserver<T> observer)
            {
                // this.Trace("start");

                var subscription = Source.Subscribe(observer);

                // this.Trace("done");

                return subscription;
            }

            public void SetFirstObserver(IObserver<T> observer)
            {
                // this.Trace("start");

                if (FirstObserver is not null)
                    throw new InvalidOperationException("first observer already subscribed");
                FirstObserver = observer;

                // this.Trace("done");
            }

            public void SetFirstSubscription(IDisposable subscription)
            {
                // this.Trace("start");

                if (!ReferenceEquals(FirstSubscription, Disposable.Empty))
                    throw new InvalidOperationException("first subscription already set");

                FirstSubscription = subscription;
                FirstObserver = null;
                _dataCts.Cancel();

                // this.Trace("done");
            }

            public void DisposeFirstSubscription()
            {
                // this.Trace("start");

                FirstSubscription.Dispose();

                // this.Trace("done");
            }
        }
    }
}