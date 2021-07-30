using System.Threading.Tasks;
using Annium.Extensions.Reactive.Internal.Creation.Disposable;
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

        public static IAsyncDisposableObservable<T> StaticAsyncInstance<T>(Func<ObserverContext<T>, Task<Func<Task>>> factory)
        {
            return new StaticObservableInstance<T>(factory, false);
        }

        public static IAsyncDisposableObservable<T> StaticSyncInstance<T>(Func<ObserverContext<T>, Task<Func<Task>>> factory)
        {
            return new StaticObservableInstance<T>(factory, true);
        }

        #endregion

        #region Empty disposables

        public static IDisposableObservable<T> EmptyDisposable<T>()
        {
            return new EmptyDisposable<T>();
        }

        public static IAsyncDisposableObservable<T> EmptyAsyncDisposable<T>()
        {
            return new EmptyAsyncDisposable<T>();
        }

        #endregion
    }
}