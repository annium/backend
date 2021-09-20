using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Internal;

namespace Annium.Extensions.Reactive.Internal.Creation.Instance
{
    internal class StaticObservableInstance<T> : ObservableInstanceBase<T>, IObservable<T>
    {
        internal StaticObservableInstance(
            Func<ObserverContext<T>, Task<Func<Task>>> factory,
            bool isAsync,
            CancellationToken ct
        )
        {
            if (isAsync)
                Task.Run(() => RunAsync(factory, ct));
            else
                RunAsync(factory, ct).GetAwaiter();
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            lock (Lock)
                Subscribers.Add(observer);

            return Core.Primitives.Disposable.Create(() =>
            {
                lock (Lock)
                    Subscribers.Remove(observer);
            });
        }

        private async Task RunAsync(
            Func<ObserverContext<T>, Task<Func<Task>>> factory,
            CancellationToken ct
        )
        {
            var ctx = GetObserverContext(ct);
            try
            {
                this.Trace("start, run factory");
                var disposeAsync = await factory(ctx);
                this.Trace("init disposal");
                InitDisposal();
                this.Trace("dispose");
                await disposeAsync();
                // technical micro delay to force thread switching
                await Task.Delay(1, CancellationToken.None);
                this.Trace("invoke onCompleted");
                ctx.OnCompleted();
                this.Trace("done");
            }
            catch (Exception e)
            {
                ctx.OnError(e);
            }
        }
    }
}