using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Primitives;

namespace Annium.Infrastructure.MessageBus.Node.Internal.Transport
{
    internal class InMemoryMessageBusSocket : IMessageBusSocket
    {
        private readonly IObservable<string> _observable;
        private readonly ManualResetEventSlim _gate = new(false);
        private readonly Queue<string> _messages = new();
        private readonly AsyncDisposableBox _disposable = Disposable.AsyncBox();

        public InMemoryMessageBusSocket(
            InMemoryConfiguration cfg
        )
        {
            var observable = ObservableInstance.Static<string>(CreateObservable);
            _observable = observable;
            _disposable += observable;

            _disposable += _gate;

            if (cfg.MessageBox is not null)
                _disposable += _observable.Subscribe(cfg.MessageBox.Add);
        }

        public IObservable<Unit> Send(string message)
        {
            lock (_messages)
                _messages.Enqueue(message);

            // open gate to let messages be read
            _gate.Set();

            return Observable.Return(Unit.Default);
        }

        public IDisposable Subscribe(IObserver<string> observer) => _observable.Subscribe(observer);

        private Task<Func<Task>> CreateObservable(ObserverContext<string> ctx)
        {
            try
            {
                while (!ctx.Ct.IsCancellationRequested)
                {
                    _gate.Wait(ctx.Ct);
                    _gate.Reset();

                    var messages = new List<string>();
                    lock (_messages)
                    {
                        while (_messages.Count > 0)
                            messages.Add(_messages.Dequeue());
                    }

                    foreach (var item in messages)
                        ctx.OnNext(item);
                }
            }
            // token was canceled
            catch (OperationCanceledException)
            {
                ctx.OnCompleted();
            }
            catch (Exception e)
            {
                ctx.OnError(e);
            }

            return Task.FromResult<Func<Task>>(() => Task.CompletedTask);
        }

        public ValueTask DisposeAsync()
        {
            return _disposable.DisposeAsync();
        }
    }
}