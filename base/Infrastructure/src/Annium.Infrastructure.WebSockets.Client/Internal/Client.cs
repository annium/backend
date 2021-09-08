using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Primitives;
using Annium.Infrastructure.WebSockets.Domain.Responses;
using Annium.Logging.Abstractions;
using Annium.Net.WebSockets;

namespace Annium.Infrastructure.WebSockets.Client.Internal
{
    internal class Client : ClientBase<ClientWebSocket>, IClient
    {
        public event Func<Task> ConnectionLost = () => Task.CompletedTask;
        public event Func<Task> ConnectionRestored = () => Task.CompletedTask;
        private readonly IClientConfiguration _configuration;
        private readonly DisposableBox _disposable = Disposable.Box();
        private bool _isDisposed;
        private TaskCompletionSource<object?> _connectionTcs = new();

        public Client(
            ITimeProvider timeProvider,
            Serializer serializer,
            IClientConfiguration configuration,
            ILoggerFactory loggerFactory
        ) : base(
            new ClientWebSocket(configuration.WebSocketOptions, loggerFactory.GetLogger<ClientWebSocket>()),
            timeProvider,
            serializer,
            configuration,
            loggerFactory.GetLogger<Client>()
        )
        {
            _configuration = configuration;
            Socket.ConnectionLost += () => ConnectionLost.Invoke();
            Socket.ConnectionRestored += async () =>
            {
                this.Log().Trace("wait for ConnectionReadyNotification");
                await WaitConnectionReadyAsync(CancellationToken.None);
                this.Log().Trace("invoke ConnectionRestored");
                await ConnectionRestored.Invoke();
                this.Log().Trace("done ConnectionRestored");
            };
            _disposable += Listen<ConnectionReadyNotification>().Subscribe(_ => HandleConnectionReady());
        }

        public async Task ConnectAsync(CancellationToken ct = default)
        {
            this.Log().Trace("start");
            await Task.WhenAll(
                WaitConnectionReadyAsync(ct),
                Socket.ConnectAsync(_configuration.Uri, ct)
            );
            this.Log().Trace("done");
        }

        public Task DisconnectAsync() =>
            Socket.DisconnectAsync();


        public override async ValueTask DisposeAsync()
        {
            if (_isDisposed)
            {
                this.Log().Trace("already disposed");
                return;
            }

            this.Log().Trace("start");
            _disposable.Dispose();
            this.Log().Trace("dispose base");
            await base.DisposeAsync();
            this.Log().Trace("disconnect socket");
            await Socket.DisconnectAsync();
            this.Log().Trace("dispose socket");
            await Socket.DisposeAsync();
            this.Log().Trace("done");

            _isDisposed = true;
        }

        private void HandleConnectionReady()
        {
            _connectionTcs.SetResult(null);
            _connectionTcs = new();
        }

        private async Task WaitConnectionReadyAsync(CancellationToken ct)
        {
            this.Log().Trace("start");

            try
            {
                await Task.Run(() => _connectionTcs.Task, ct);
            }
            catch (OperationCanceledException)
            {
            }

            this.Log().Trace("done");
        }
    }
}