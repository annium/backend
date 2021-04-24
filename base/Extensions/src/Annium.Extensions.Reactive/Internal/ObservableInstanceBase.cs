using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Annium.Core.Primitives;

namespace Annium.Extensions.Reactive.Internal
{
    internal abstract class ObservableInstanceBase<T>
    {
        protected readonly HashSet<IObserver<T>> Subscribers = new();
        protected readonly object Lock = new();
        private bool _isCompleted;
        private bool _isDisposed;

        protected ObserverContext<T> GetObserverContext(CancellationToken ct) => new(OnNext, OnError, OnCompleted, ct);

        protected void BeforeDispose()
        {
            EnsureNotDisposed();
            _isDisposed = true;
        }

        protected void AfterDispose()
        {
            if (!_isCompleted)
                throw new InvalidOperationException("Observable not completed");
        }

        private void OnNext(T value)
        {
            EnsureNotDisposed();

            foreach (var observer in GetSubscribersSlice())
                observer.OnNext(value);
        }

        private void OnError(Exception exception)
        {
            EnsureNotDisposed();

            foreach (var observer in GetSubscribersSlice())
                observer.OnError(exception);
        }

        private void OnCompleted()
        {
            if (_isCompleted)
                throw new InvalidOperationException("Observable already completed");
            _isCompleted = true;

            foreach (var observer in GetSubscribersSlice())
                observer.OnCompleted();
        }

        private void EnsureNotDisposed()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FriendlyName());
        }

        private IReadOnlyCollection<IObserver<T>> GetSubscribersSlice()
        {
            lock (Lock)
                return Subscribers.ToArray();
        }
    }
}