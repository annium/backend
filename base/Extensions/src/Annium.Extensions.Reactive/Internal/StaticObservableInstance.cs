using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Primitives;

namespace Annium.Extensions.Reactive.Internal
{
    internal class StaticObservableInstance<T> : ObservableInstanceBase<T>, IObservableInstance<T>
    {
        private readonly Task _factoryTask;
        private readonly CancellationTokenSource _cts = new();

        internal StaticObservableInstance(
            Func<ObserverContext<T>, Task> factory
        )
        {
            _factoryTask = Task.Run(() => factory(GetObserverContext(_cts.Token)));
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
            await _factoryTask;

            AfterDispose();
        }
    }
}