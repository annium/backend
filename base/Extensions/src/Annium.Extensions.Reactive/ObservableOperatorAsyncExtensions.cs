using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace System
{
    public static class ObservableOperatorAsyncExtensions
    {
        public static IObservable<TResult> SelectParallelAsync<TSource, TResult>(
            this IObservable<TSource> source,
            Func<TSource, Task<TResult>> selector
        )
            => source.SelectMany(x => Observable.FromAsync(() => selector(x)));

        public static IObservable<TResult> SelectParallelAsync<TSource, TResult>(
            this IObservable<TSource> source,
            Func<TSource, int, Task<TResult>> selector
        )
            => source.SelectMany((x, i) => Observable.FromAsync(() => selector(x, i)));

        public static IObservable<Unit> SelectParallelAsync<TSource>(
            this IObservable<TSource> source,
            Func<TSource, Task> selector
        )
            => source.SelectMany(x => Observable.FromAsync(() => selector(x)));

        public static IObservable<Unit> SelectParallelAsync<TSource>(
            this IObservable<TSource> source,
            Func<TSource, int, Task> selector
        )
            => source.SelectMany((x, i) => Observable.FromAsync(() => selector(x, i)));

        public static IObservable<TResult> SelectSequentialAsync<TSource, TResult>(
            this IObservable<TSource> source,
            Func<TSource, Task<TResult>> selector
        )
            => source.Select(x => Observable.FromAsync(() => selector(x))).Concat();

        public static IObservable<TResult> SelectSequentialAsync<TSource, TResult>(
            this IObservable<TSource> source,
            Func<TSource, int, Task<TResult>> selector
        )
            => source.Select((x, i) => Observable.FromAsync(() => selector(x, i))).Concat();

        public static IObservable<Unit> SelectSequentialAsync<TSource>(
            this IObservable<TSource> source,
            Func<TSource, Task> selector
        )
            => source.Select(x => Observable.FromAsync(() => selector(x))).Concat();

        public static IObservable<Unit> SelectSequentialAsync<TSource>(
            this IObservable<TSource> source,
            Func<TSource, int, Task> selector
        )
            => source.Select((x, i) => Observable.FromAsync(() => selector(x, i))).Concat();

        public static IObservable<TSource> CatchAsync<TSource, TException>(
            this IObservable<TSource> source,
            Func<TException, Task<IObservable<TSource>>> handler
        )
            where TException : Exception
            => source.Catch<TSource, TException>(e => Observable.FromAsync(() => handler(e)).SelectMany(x => x));
    }
}