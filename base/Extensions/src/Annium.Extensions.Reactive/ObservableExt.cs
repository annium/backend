using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Reactive.Internal.Creation.Instance;

namespace System.Reactive.Linq
{
    public static class ObservableExt
    {
        #region Instance

        // public static IObservableInstance<T> Dynamic<T>(Func<ObserverContext<T>, Task<Func<Task>>> factory)
        // {
        //     return new DynamicObservableInstance<T>(factory);
        // }

        public static IObservable<T> StaticAsyncInstance<T>(
            Func<ObserverContext<T>, Task<Func<Task>>> factory,
            CancellationToken ct
        )
        {
            return new StaticObservableInstance<T>(factory, true, ct);
        }

        public static IObservable<T> StaticSyncInstance<T>(
            Func<ObserverContext<T>, Task<Func<Task>>> factory,
            CancellationToken ct
        )
        {
            return new StaticObservableInstance<T>(factory, false, ct);
        }

        #endregion
    }
}