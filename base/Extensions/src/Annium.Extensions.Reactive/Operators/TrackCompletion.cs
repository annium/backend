using System.Collections.Generic;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Annium.Core.Internal;
using Annium.Core.Primitives;
using Annium.Diagnostics.Debug;

namespace System
{
    public static class TrackCompletionOperatorExtensions
    {
        public static IObservable<T> TrackCompletion<T>(
            this IObservable<T> source
        )
        {
            var ctx = new Context<T>(source);

            source.Subscribe(delegate { }, () => Complete(ctx), ctx.CompletionCt);

            return Observable.Create(CreateObservable(ctx));
        }

        private static Func<IObserver<T>, IDisposable> CreateObservable<T>(Context<T> ctx) => observer =>
        {
            lock (ctx)
            {
                var target = $"{observer}#{observer.GetId()}";
                ctx.Trace($"{target} - subscribe");

                if (ctx.IsCompleted)
                {
                    observer.OnCompleted();
                    ctx.Trace($"{target} - completed");

                    return Disposable.Empty;
                }

                ctx.Trace($"{target} - attached");

                return ctx.Subscribe(observer);
            }
        };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Complete<T>(Context<T> ctx)
        {
            lock (ctx)
            {
                ctx.Trace("start");

                if (ctx.IsCompleted)
                    throw new InvalidOperationException("Can't complete, ctx already completed");

                ctx.Complete();

                ctx.Trace("done");
            }
        }

        private class Context<T>
        {
            public IObservable<T> Source { get; }
            public bool IsCompleted { get; private set; }
            public CancellationToken CompletionCt => _completionCts.Token;
            private readonly List<IObserver<T>> _incompleteObservers = new();
            private readonly CancellationTokenSource _completionCts = new();

            public Context(
                IObservable<T> source
            )
            {
                Source = source;
            }

            public void Complete()
            {
                this.Trace("start");

                if (IsCompleted)
                    throw new InvalidOperationException("source already completed");
                IsCompleted = true;

                foreach (var observer in _incompleteObservers)
                    observer.OnCompleted();
                _incompleteObservers.Clear();

                _completionCts.Cancel();

                this.Trace("done");
            }

            public IDisposable Subscribe(IObserver<T> observer)
            {
                this.Trace("start");

                _incompleteObservers.Add(observer);
                var subscription = Source.Subscribe(observer.OnNext, observer.OnError);

                this.Trace("done");

                return subscription;
            }
        }
    }
}