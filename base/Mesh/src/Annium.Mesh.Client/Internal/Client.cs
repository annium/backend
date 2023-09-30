using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Mesh.Domain.Responses;
using Annium.Net.WebSockets;

namespace Annium.Mesh.Client.Internal;

internal class Client : ClientBase<ClientWebSocket>, IClient
{
    public event Func<Task> ConnectionLost = () => Task.CompletedTask;
    public event Func<Task> ConnectionRestored = () => Task.CompletedTask;
    private readonly IClientConfiguration _configuration;
    private readonly DisposableBox _disposable;
    private bool _isDisposed;
    private TaskCompletionSource _connectionTcs = new();

    public Client(
        ITimeProvider timeProvider,
        Serializer serializer,
        IClientConfiguration configuration,
        ILogger logger
    ) : base(
        new ClientWebSocket(configuration.WebSocketOptions, logger),
        timeProvider,
        serializer,
        configuration,
        logger
    )
    {
        _configuration = configuration;
        _disposable = Disposable.Box(logger);
        Socket.OnDisconnected += _ => ConnectionLost.Invoke();
        Socket.OnConnected += async () =>
        {
            this.Trace("wait for ConnectionReadyNotification");
            await WaitConnectionReadyAsync(CancellationToken.None);
            this.Trace("invoke ConnectionRestored");
            await ConnectionRestored.Invoke();
            this.Trace("done ConnectionRestored");
        };
        _disposable += Listen<ConnectionReadyNotification>().Subscribe(_ => HandleConnectionReady());
    }

    public async Task ConnectAsync(CancellationToken ct = default)
    {
        this.Trace("start");
        Socket.Connect(_configuration.Uri);
        await WaitConnectionReadyAsync(ct);
        this.Trace("done");
    }

    public Task DisconnectAsync()
    {
        Socket.Disconnect();

        return Task.CompletedTask;
    }


    public override async ValueTask DisposeAsync()
    {
        if (_isDisposed)
        {
            this.Trace("already disposed");
            return;
        }

        this.Trace("start");
        _disposable.Dispose();
        this.Trace("dispose base");
        await base.DisposeAsync();
        this.Trace("disconnect socket");
        Socket.Disconnect();
        this.Trace("done");

        _isDisposed = true;
    }

    private void HandleConnectionReady()
    {
        _connectionTcs.SetResult();
        _connectionTcs = new();
    }

    private async Task WaitConnectionReadyAsync(CancellationToken ct)
    {
        this.Trace("start");

        try
        {
            await Task.Run(() => _connectionTcs.Task, ct);
        }
        catch (OperationCanceledException)
        {
        }

        this.Trace("done");
    }
}