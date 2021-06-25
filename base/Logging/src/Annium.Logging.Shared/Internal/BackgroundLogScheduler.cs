using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Annium.Core.Internal;

namespace Annium.Logging.Shared.Internal
{
    internal class BackgroundLogScheduler : ILogScheduler, IAsyncDisposable
    {
        public Func<LogMessage, bool> Filter { get; }
        private int Count => _reader.CanCount ? _reader.Count : -1;
        private bool _isDisposed;
        private readonly ChannelReader<LogMessage> _reader;
        private readonly ChannelWriter<LogMessage> _writer;
        private readonly IObservableInstance<LogMessage> _observable;
        private readonly IDisposable _subscription;

        public BackgroundLogScheduler(
            Func<LogMessage, bool> filter,
            IAsyncLogHandler handler,
            LogRouteConfiguration configuration
        )
        {
            Filter = filter;

            var channel = Channel.CreateUnbounded<LogMessage>(new UnboundedChannelOptions
            {
                AllowSynchronousContinuations = true,
                SingleReader = true
            });
            _writer = channel.Writer;
            _reader = channel.Reader;
            _observable = ObservableInstance.Static<LogMessage>(Run);
            _subscription = _observable
                .Buffer(configuration.BufferTime, configuration.BufferCount)
                .DoParallelAsync(async x => await handler.Handle(x.ToArray()))
                .Subscribe();
        }

        public void Handle(LogMessage message)
        {
            EnsureNotDisposed();

            if (!_writer.TryWrite(message))
                throw new InvalidOperationException("Message must have been written to channel");
        }

        private async Task<Func<Task>> Run(ObserverContext<LogMessage> ctx)
        {
            this.Trace("start");
            // normal mode - runs task immediately or waits for one
            while (!Volatile.Read(ref _isDisposed))
            {
                try
                {
                    this.Trace("wait for msg");
                    var message = await _reader.ReadAsync(ctx.Ct);
                    ctx.OnNext(message);
                }
                catch (ChannelClosedException)
                {
                    this.Trace("cancelled");
                    break;
                }
                catch (OperationCanceledException)
                {
                    this.Trace("cancelled");
                    break;
                }
            }

            // shutdown mode - handle only left tasks
            this.Trace($"handle {Count} messages left");
            while (true)
            {
                this.Trace("get task");
                if (_reader.TryRead(out var message))
                    ctx.OnNext(message);
                else
                    break;
            }

            ctx.OnCompleted();

            return () => Task.CompletedTask;
        }

        public async ValueTask DisposeAsync()
        {
            this.Trace("start");
            EnsureNotDisposed();
            Volatile.Write(ref _isDisposed, true);
            _writer.Complete();
            this.Trace("wait for reader completion");
            await _reader.Completion;
            this.Trace($"wait for {Count} messages(s) to finish");
            await _observable.DisposeAsync();
            _subscription.Dispose();
            this.Trace("done");
        }

        private void EnsureNotDisposed()
        {
            if (Volatile.Read(ref _isDisposed))
                throw new InvalidOperationException("Log scheduler is already disposed");
        }
    }
}