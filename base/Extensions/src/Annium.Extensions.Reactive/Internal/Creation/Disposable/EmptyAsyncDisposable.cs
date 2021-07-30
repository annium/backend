using System;
using System.Threading.Tasks;

namespace Annium.Extensions.Reactive.Internal.Creation.Disposable
{
    public class EmptyAsyncDisposable<T> : IAsyncDisposableObservable<T>
    {
        public IDisposable Subscribe(IObserver<T> observer)
        {
            observer.OnCompleted();
            return Core.Primitives.Disposable.Empty;
        }

        public ValueTask DisposeAsync() => new(Task.CompletedTask);
    }
}