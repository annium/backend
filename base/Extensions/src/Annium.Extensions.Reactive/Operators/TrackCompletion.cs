using System.Collections.Generic;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Annium.Core.Internal;
using Annium.Core.Primitives;

namespace System
{
    public static class TrackCompletionOperatorExtensions
    {
        public static IObservable<T> TrackCompletion<T>(
            this IObservable<T> source
        )
        {
            var ctx = new CompletionContext<T>(source);

            source.Subscribe(delegate { }, ctx.Complete, ctx.CompletionCt);

            ctx.Trace("create observable");

            return Observable.Create(CreateObservable(ctx));
        }

        private static Func<IObserver<T>, IDisposable> CreateObservable<T>(CompletionContext<T> ctx) => observer =>
        {
            var target = observer.GetFullId();
            ctx.Trace($"{target} - handle");

            if (!ctx.IsCompleted)
                return ctx.Subscribe(observer);

            ctx.Trace($"{target} - complete");
            observer.OnCompleted();
            ctx.Trace($"{target} - completed");

            return Disposable.Empty;
        };

        private class CompletionContext<T>
        {
            public bool IsCompleted { get; private set; }
            public CancellationToken CompletionCt => _completionCts.Token;
            private readonly IObservable<T> _source;
            private readonly List<IObserver<T>> _incompleteObservers = new();
            private readonly CancellationTokenSource _completionCts = new();

            public CompletionContext(
                IObservable<T> source
            )
            {
                _source = source;
            }

            public IDisposable Subscribe(IObserver<T> observer)
            {
                lock (this)
                {
                    Trace("start");

                    _incompleteObservers.Add(observer);
                    var subscription = _source.Subscribe(observer.OnNext, observer.OnError);

                    Trace("done");

                    return subscription;
                }
            }

            public void Complete()
            {
                Trace("start");

                IReadOnlyCollection<IObserver<T>> observers;
                lock (this)
                {
                    if (IsCompleted)
                        throw new InvalidOperationException("source already completed");
                    IsCompleted = true;

                    Trace($"complete {_incompleteObservers.Count} observers");
                    observers = _incompleteObservers.ToArray();
                    _incompleteObservers.Clear();
                }

                foreach (var observer in observers)
                {
                    Trace($"complete {observer.GetFullId()}");
                    observer.OnCompleted();
                }

                Trace("cancel cts");
                _completionCts.Cancel();

                Trace("done");
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
    }
}