using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Primitives;

namespace Annium.Infrastructure.MessageBus.Node.Internal
{
    internal class InMemoryMessageBusSocket : IMessageBusSocket
    {
        private readonly IObservable<string> _observable;
        private readonly ManualResetEventSlim _gate = new(false);
        private readonly Queue<string> _messages = new();
        private readonly DisposableBox _disposable = Disposable.Box();

        public InMemoryMessageBusSocket(
            InMemoryConfiguration cfg
        )
        {
            _observable = Observable.Create<string>(CreateObservable).Publish().RefCount();
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

        private Task CreateObservable(
            IObserver<string> observer,
            CancellationToken ct
        ) => Task.Run(() =>
        {
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    _gate.Wait(ct);
                    _gate.Reset();

                    var messages = new List<string>();
                    lock (_messages)
                    {
                        while (_messages.Count > 0)
                            messages.Add(_messages.Dequeue());
                    }

                    foreach (var item in messages)
                        observer.OnNext(item);
                }
            }
            // token was canceled
            catch (OperationCanceledException)
            {
                observer.OnCompleted();
            }
            catch (Exception e)
            {
                observer.OnError(e);
            }
        }, ct);

        public ValueTask DisposeAsync()
        {
            _disposable.Dispose();

            return new(Task.CompletedTask);
        }
    }
}