using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Primitives;

namespace Annium.Extensions.Reactive.Internal
{
    internal class StaticObservableInstance<T> : ObservableInstanceBase<T>, IObservableInstance<T>
    {
        private readonly Task<Func<Task>> _factoryTask;
        private readonly CancellationTokenSource _cts = new();

        internal StaticObservableInstance(
            Func<ObserverContext<T>, Task<Func<Task>>> factory,
            bool sync
        )
        {
            _factoryTask = sync
                ? factory(GetObserverContext(_cts.Token))
                : Task.Run(() => factory(GetObserverContext(_cts.Token)));
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            lock (Lock)
                Subscribers.Add(observer);

            return Disposable.Create(() =>
            {
                lock (Lock)
                    Subscribers.Remove(observer);
            });
        }

        public async ValueTask DisposeAsync()
        {
            BeforeDispose();

            _cts.Cancel();
            var disposeAsync = await _factoryTask;
            await disposeAsync();

            AfterDispose();
        }
    }
}