using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;
using Annium.Core.Primitives;

namespace Annium.Infrastructure.MessageBus.Node.Internal.Transport
{
    internal class InMemoryMessageBusSocket : IMessageBusSocket
    {
        private readonly IObservableInstance<string> _observable;
        private readonly ChannelWriter<string> _writer;
        private readonly ChannelReader<string> _reader;
        private readonly AsyncDisposableBox _disposable = Disposable.AsyncBox();

        public InMemoryMessageBusSocket(
            InMemoryConfiguration cfg
        )
        {
            var taskChannel = Channel.CreateUnbounded<string>(new UnboundedChannelOptions
            {
                AllowSynchronousContinuations = true,
                SingleReader = true
            });
            _writer = taskChannel.Writer;
            _reader = taskChannel.Reader;

            _disposable += _observable = ObservableInstance.StaticSync<string>(CreateObservable);

            if (cfg.MessageBox is not null)
                _disposable += _observable.Subscribe(cfg.MessageBox.Add);
        }

        public IObservable<Unit> Send(string message)
        {
            if (!_writer.TryWrite(message))
                throw new InvalidOperationException("Message must have been written");

            return Observable.Return(Unit.Default);
        }

        public IDisposable Subscribe(IObserver<string> observer) => _observable.Subscribe(observer);

        private async Task<Func<Task>> CreateObservable(ObserverContext<string> ctx)
        {
            try
            {
                while (!ctx.Ct.IsCancellationRequested)
                {
                    var message = await _reader.ReadAsync(ctx.Ct);

                    ctx.OnNext(message);
                }
            }
            // token was canceled
            catch (OperationCanceledException)
            {
                ctx.OnCompleted();
            }
            catch (ChannelClosedException)
            {
                ctx.OnCompleted();
            }
            catch (Exception e)
            {
                ctx.OnError(e);
            }

            return () => Task.CompletedTask;
        }

        public ValueTask DisposeAsync()
        {
            return _disposable.DisposeAsync();
        }
    }
}