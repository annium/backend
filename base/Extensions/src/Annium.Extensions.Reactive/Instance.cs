using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Primitives;

namespace System
{
    public static class ObservableInstance
    {
        public static IObservable<T> Create<T>(Func<Action<T>, Action<Exception>, CancellationToken, Task> factory)
        {
            return new ObservableInstance<T>(factory);
        }
    }

    internal class ObservableInstance<T> : IObservable<T>, IAsyncDisposable
    {
        private readonly Func<Action<T>, Action<Exception>, CancellationToken, Task> _factory;
        private readonly HashSet<IObserver<T>> _subscribers = new();
        private readonly object _lock = new();
        private Task _factoryTask = Task.CompletedTask;
        private CancellationTokenSource _factoryCts = new();
        private bool _isDisposed;

        internal ObservableInstance(
            Func<Action<T>, Action<Exception>, CancellationToken, Task> factory
        )
        {
            _factory = WrapFactory(factory);
        }


        public IDisposable Subscribe(IObserver<T> observer)
        {
            lock (_lock)
            {
                _subscribers.Add(observer);

                // init factory if first subscriber
                if (_subscribers.Count == 1)
                {
                    var factoryTask = _factoryTask.IsCompleted ? Task.CompletedTask : _factoryTask;
                    _factoryTask = Task.Run(async () =>
                    {
                        await factoryTask;
                        await _factory(OnNext, OnError, _factoryCts.Token);
                    });
                }
            }

            return Disposable.Create(() =>
            {
                lock (_lock)
                {
                    _subscribers.Remove(observer);

                    // cancel factory if last subscriber
                    if (_subscribers.Count == 0 && !_factoryCts.IsCancellationRequested)
                        _factoryCts.Cancel();
                }
            });
        }

        public async ValueTask DisposeAsync()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FriendlyName());
            _isDisposed = true;

            Task factoryTask;
            lock (_lock) factoryTask = _factoryTask;

            await factoryTask;
        }

        private void OnNext(T value)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FriendlyName());

            foreach (var observer in _subscribers)
                observer.OnNext(value);
        }

        private void OnError(Exception exception)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FriendlyName());

            foreach (IObserver<T> observer in _subscribers)
                observer.OnError(exception);
        }

        private void OnCompleted()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FriendlyName());

            foreach (IObserver<T> observer in _subscribers)
                observer.OnCompleted();
        }

        private Func<Action<T>, Action<Exception>, CancellationToken, Task> WrapFactory(
            Func<Action<T>, Action<Exception>, CancellationToken, Task> factory
        ) => async (onNext, onError, ct) =>
        {
            _factoryCts = new CancellationTokenSource();
            await factory(onNext, onError, _factoryCts.Token);
        };
    }
}