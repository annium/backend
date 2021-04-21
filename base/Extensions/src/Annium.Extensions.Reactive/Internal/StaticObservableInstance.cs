using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Primitives;

namespace Annium.Extensions.Reactive.Internal
{
    internal class StaticObservableInstance<T> : IObservableInstance<T>
    {
        private readonly HashSet<IObserver<T>> _subscribers = new();
        private readonly object _lock = new();
        private readonly Task _factoryTask;
        private readonly CancellationTokenSource _cts = new();
        private bool _isDisposed;

        internal StaticObservableInstance(
            Func<ObserverContext<T>, Task> factory
        )
        {
            _factoryTask = factory(new ObserverContext<T>(OnNext, OnError, OnCompleted, _cts.Token));
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            lock (_lock)
                _subscribers.Add(observer);

            return Disposable.Create(() =>
            {
                lock (_lock)
                    _subscribers.Remove(observer);
            });
        }

        public async ValueTask DisposeAsync()
        {
            EnsureNotDisposed();
            _isDisposed = true;

            OnCompleted();
            _cts.Cancel();
            await _factoryTask;
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
            EnsureNotDisposed();

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
            lock (_lock)
                return _subscribers.ToArray();
        }
    }
}