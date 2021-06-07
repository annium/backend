using System.Threading.Tasks;
using Annium.Extensions.Reactive.Internal;

namespace System
{
    public static class ObservableInstance
    {
        // public static IObservableInstance<T> Dynamic<T>(Func<ObserverContext<T>, Task<Func<Task>>> factory)
        // {
        //     return new DynamicObservableInstance<T>(factory);
        // }

        public static IObservableInstance<T> Static<T>(Func<ObserverContext<T>, Task<Func<Task>>> factory)
        {
            return new StaticObservableInstance<T>(factory);
        }
    }

    public interface IObservableInstance<T> : IObservable<T>, IAsyncDisposable
    {
    }
}