using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Primitives;

namespace Annium.Extensions.Reactive.Internal
{
    internal class DynamicObservableInstance<T> : ObservableInstanceBase<T>, IObservableInstance<T>
    {
        private readonly Func<ObserverContext<T>, Task> _factory;
        private Task _factoryTask = Task.CompletedTask;
        private CancellationTokenSource _factoryCts = new();

        internal DynamicObservableInstance(
            Func<ObserverContext<T>, Task> factory
        )
        {
            _factory = WrapFactory(factory);
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            lock (Lock)
            {
                Subscribers.Add(observer);

                // init factory if first subscriber
                if (Subscribers.Count == 1)
                {
                    var factoryTask = _factoryTask.IsCompleted ? Task.CompletedTask : _factoryTask;
                    _factoryTask = Task.Run(async () =>
                    {
                        await factoryTask;
                        await _factory(GetObserverContext(_factoryCts.Token));
                    });
                }
            }

            return Disposable.Create(() =>
            {
                lock (Lock)
                {
                    Subscribers.Remove(observer);

                    // cancel factory if last subscriber
                    if (Subscribers.Count == 0 && !_factoryCts.IsCancellationRequested)
                        _factoryCts.Cancel();
                }
            });
        }

        public async ValueTask DisposeAsync()
        {
            BeforeDispose();

            _factoryCts.Cancel();
            Task factoryTask;
            lock (Lock) factoryTask = _factoryTask;

            await factoryTask;

            AfterDispose();
        }

        private Func<ObserverContext<T>, Task> WrapFactory(
            Func<ObserverContext<T>, Task> factory
        ) => async ctx =>
        {
            _factoryCts = new CancellationTokenSource();
            await factory(ctx with { Token = _factoryCts.Token });
        };
    }
}