using System.Reactive.Linq;
using System.Threading.Tasks;

namespace System
{
    public static class ObservableOperatorAsyncExtensions
    {
        public static IObservable<TResult> SelectAsync<TSource, TResult>(
            this IObservable<TSource> source,
            Func<TSource, Task<TResult>> selector
        )
            => source.SelectMany(x => Observable.FromAsync(() => selector(x)));

        public static IObservable<TResult> SelectAsync<TSource, TResult>(
            this IObservable<TSource> source,
            Func<TSource, int, Task<TResult>> selector
        )
            => source.SelectMany((x, i) => Observable.FromAsync(() => selector(x, i)));

        public static IObservable<TSource> CatchAsync<TSource, TException>(
            this IObservable<TSource> source,
            Func<TException, Task<IObservable<TSource>>> handler
        )
            where TException : Exception
            => source.Catch<TSource, TException>(e => Observable.FromAsync(() => handler(e)).SelectMany(x => x));
    }
}