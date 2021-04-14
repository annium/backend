using System.Reactive.Linq;
using System.Threading.Tasks;
using Annium.Core.Primitives;

namespace System
{
    public static class ObservableOperatorAsyncExtensions
    {
        public static IObservable<TResult> Select<TSource, TResult>(
            this IObservable<TSource> source,
            Func<TSource, Task<TResult>> selector
        )
            => source.Select(x => selector(x).Await());

        public static IObservable<TResult> Select<TSource, TResult>(
            this IObservable<TSource> source,
            Func<TSource, int, Task<TResult>> selector
        )
            => source.Select((x, i) => selector(x, i).Await());

        public static IObservable<TSource> Catch<TSource, TException>(
            this IObservable<TSource> source,
            Func<TException, Task<IObservable<TSource>>> handler
        )
            where TException : Exception
            => source.Catch<TSource, TException>(e => handler(e).Await());
    }
}