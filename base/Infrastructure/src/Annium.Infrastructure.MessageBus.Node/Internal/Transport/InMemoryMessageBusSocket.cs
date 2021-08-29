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
        private readonly IAsyncDisposableObservable<string> _observable;
        private readonly ChannelWriter<string> _messageWriter;
        private readonly ChannelReader<string> _messageReader;
        private readonly AsyncDisposableBox _disposable = Disposable.AsyncBox();

        public InMemoryMessageBusSocket()
        {
            var taskChannel = Channel.CreateUnbounded<string>(new UnboundedChannelOptions
            {
                AllowSynchronousContinuations = true,
                SingleWriter = true,
                SingleReader = true
            });
            _messageWriter = taskChannel.Writer;
            _messageReader = taskChannel.Reader;

            _disposable += _observable = ObservableExt.StaticSyncInstance<string>(CreateObservable);
        }

        public IObservable<Unit> Send(string message)
        {
            lock (_messageWriter)
                if (!_messageWriter.TryWrite(message))
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
                    var message = await _messageReader.ReadAsync(ctx.Ct);

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