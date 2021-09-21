using System.Collections.Generic;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Annium.Core.Internal;
using Annium.Core.Primitives;
using Annium.Diagnostics.Debug;
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

            ctx.Trace("init buffering");

            return Observable.Create(CreateObservable(ctx));
        }

        private static Func<IObserver<T>, IDisposable> CreateObservable<T>(Context<T> ctx) => observer =>
        {
            lock (ctx)
            {
                ctx.Trace($"subscribe {observer}#{observer.GetId()}");
                if (ctx.IsFlushed)
                {
                    if (ctx.IsCompleted)
                    {
                        ctx.Trace($"flushed, completed {observer}#{observer.GetId()}");
                        observer.OnCompleted();

                        return Disposable.Empty;
                    }

                    ctx.Trace($"flushed, attached {observer}#{observer.GetId()}");

                    return ctx.Source.Subscribe(observer);
                }

                ctx.Trace($"pipe to: {observer}#{observer.GetId()}");

                while (ctx.Events.TryDequeue(out var e))
                    observer.Handle(e);

                ctx.Flush();

                if (ctx.IsCompleted)
                {
                    ctx.Trace($"piped, completed {observer}#{observer.GetId()}");
                    return Disposable.Empty;
                }

                ctx.Trace($"piped, attached {observer}#{observer.GetId()}");
                ctx.SetFirstObserver(observer);

                return Disposable.Create(ctx.DisposeFirstSubscription);
            }
        };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Add<T>(Context<T> ctx, T data, Exception? error)
        {
            lock (ctx)
            {
                if (ctx.IsCompleted)
                    throw new InvalidOperationException($"Can't add: {data} - {error}, ctx already completed");

                var e = new ObservableEvent<T>(data, error, false);
                if (ctx.FirstObserver is null)
                {
                    ctx.Trace($"enqueue: {data} - {error}");
                    ctx.Events.Enqueue(e);
                }
                else
                {
                    ctx.Trace($"process: {data} - {error}");
                    ctx.FirstObserver.Handle(e);
                    ctx.SubscribeFirstObserver();
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Complete<T>(Context<T> ctx)
        {
            lock (ctx)
            {
                if (ctx.IsCompleted)
                    throw new InvalidOperationException("Can't complete, ctx already completed");

                if (ctx.FirstObserver is not null)
                    ctx.CompleteFirstObserver();
                else if (!ctx.IsFlushed)
                {
                    ctx.Trace("enqueue: completed");
                    ctx.Events.Enqueue(new ObservableEvent<T>(default!, null, true));
                }

                ctx.Complete();
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
                if (IsFlushed)
                    throw new InvalidOperationException("source already flushed");
                IsFlushed = true;
            }

            public void Complete()
            {
                if (IsCompleted)
                    throw new InvalidOperationException("source already completed");
                IsCompleted = true;
                _completionCts.Cancel();
            }

            public void SetFirstObserver(IObserver<T> observer)
            {
                if (FirstObserver is not null)
                    throw new InvalidOperationException("first observer already subscribed");
                FirstObserver = observer;
            }

            public void SubscribeFirstObserver()
            {
                if (FirstObserver is null)
                    throw new InvalidOperationException("first observer not set");

                FirstSubscription = Source.Subscribe(FirstObserver);
                FirstObserver = null;
                _dataCts.Cancel();
            }

            public void DisposeFirstSubscription()
            {
                FirstSubscription.Dispose();
            }

            public void CompleteFirstObserver()
            {
                if (FirstObserver is null)
                    throw new InvalidOperationException("first observer not set");

                this.Trace("completed");
                FirstObserver.OnCompleted();
                FirstObserver = null;
            }
        }
    }
}