using System;

namespace Annium.Extensions.Reactive.Internal.Creation.Disposable
{
    internal class EmptyDisposable<T> : IDisposableObservable<T>
    {
        public IDisposable Subscribe(IObserver<T> observer)
        {
            observer.OnCompleted();
            return Core.Primitives.Disposable.Empty;
        }

        public void Dispose()
        {
        }
    }
}