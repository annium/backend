using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Primitives;

namespace System
{
    public static class ObservableSubscribeAsyncExtensions
    {
        public static void SubscribeAsync<T>(
            this IObservable<T> source,
            Func<T, Task> onNext,
            CancellationToken ct
        )
            => source.Subscribe(x => onNext(x).Await(), Stubs.Throw, Stubs.Noop, ct);

        public static void SubscribeAsync<T>(
            this IObservable<T> source,
            Func<T, Task> onNext,
            Func<Exception, Task> onError,
            CancellationToken ct
        )
            => source.Subscribe(x => onNext(x).Await(), x => onError(x).Await(), Stubs.Noop, ct);

        public static void SubscribeAsync<T>(
            this IObservable<T> source,
            Func<T, Task> onNext,
            Func<Task> onCompleted,
            CancellationToken ct
        )
            => source.Subscribe(x => onNext(x).Await(), Stubs.Throw, () => onCompleted().Await(), ct);

        public static void SubscribeAsync<T>(
            this IObservable<T> source,
            Func<T, Task> onNext,
            Func<Exception, Task> onError,
            Func<Task> onCompleted,
            CancellationToken ct
        )
            => source.Subscribe(x => onNext(x).Await(), x => onError(x).Await(), () => onCompleted().Await(), ct);

        public static IDisposable SubscribeAsync<T>(
            this IObservable<T> source,
            Func<T, Task> onNext
        )
            => source.Subscribe(x => onNext(x).Await(), Stubs.Throw, Stubs.Noop);

        public static IDisposable SubscribeAsync<T>(
            this IObservable<T> source,
            Func<T, Task> onNext,
            Func<Exception, Task> onError
        )
            => source.Subscribe(x => onNext(x).Await(), x => onError(x).Await(), Stubs.Noop);

        public static IDisposable SubscribeAsync<T>(
            this IObservable<T> source,
            Func<T, Task> onNext,
            Func<Task> onCompleted
        )
            => source.Subscribe(x => onNext(x).Await(), Stubs.Throw, () => onCompleted().Await());

        public static IDisposable SubscribeAsync<T>(
            this IObservable<T> source,
            Func<T, Task> onNext,
            Func<Exception, Task> onError,
            Func<Task> onCompleted
        )
            => source.Subscribe(x => onNext(x).Await(), x => onError(x).Await(), () => onCompleted().Await());

        private static class Stubs
        {
            public static readonly Action Noop = static() => { };
            public static readonly Action<Exception> Throw = static ex => { ExceptionDispatchInfo.Capture(ex).Throw(); };
        }
    }
}