using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Primitives;

namespace System
{
    public static class ObservableInstance
    {
        public static IObservable<T> Create<T>(Func<ObserverContext<T>, Task> factory)
        {
            return new ObservableInstance<T>(factory);
        }
    }

    internal class ObservableInstance<T> : IObservable<T>, IAsyncDisposable
    {
        private readonly Func<ObserverContext<T>, Task> _factory;
        private readonly HashSet<IObserver<T>> _subscribers = new();
        private readonly object _lock = new();
        private Task _factoryTask = Task.CompletedTask;
        private CancellationTokenSource _factoryCts = new();
        private bool _isDisposed;

        internal ObservableInstance(
            Func<ObserverContext<T>, Task> factory
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
                        await _factory(new ObserverContext<T>(OnNext, OnError, OnCompleted, _factoryCts.Token));
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

            OnCompleted();
            _factoryCts.Cancel();
            Task factoryTask;
            lock (_lock) factoryTask = _factoryTask;

            await factoryTask;
        }

        private void OnNext(T value)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FriendlyName());

            foreach (var observer in _subscribers.ToArray())
                observer.OnNext(value);
        }

        private void OnError(Exception exception)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FriendlyName());

            foreach (var observer in _subscribers.ToArray())
                observer.OnError(exception);
        }

        private void OnCompleted()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FriendlyName());

            foreach (var observer in _subscribers.ToArray())
                observer.OnCompleted();
        }

        private Func<ObserverContext<T>, Task> WrapFactory(
            Func<ObserverContext<T>, Task> factory
        ) => async ctx =>
        {
            _factoryCts = new CancellationTokenSource();
            await factory(ctx with {Token = _factoryCts.Token});
        };
    }
}