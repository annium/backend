using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Debug;
using Annium.Infrastructure.WebSockets.Domain.Responses;
using Annium.Logging;
using Annium.Net.WebSockets.Obsolete;

namespace Annium.Infrastructure.WebSockets.Client.Internal;

internal class Client : ClientBase<ClientWebSocket>, IClient
{
    public event Func<Task> ConnectionLost = () => Task.CompletedTask;
    public event Func<Task> ConnectionRestored = () => Task.CompletedTask;
    private readonly IClientConfiguration _configuration;
    private readonly DisposableBox _disposable;
    private bool _isDisposed;
    private TaskCompletionSource<object?> _connectionTcs = new();

    public Client(
        ITimeProvider timeProvider,
        Serializer serializer,
        IClientConfiguration configuration,
        ILogger logger,
        ITracer tracer
    ) : base(
        new ClientWebSocket(configuration.WebSocketOptions, tracer),
        timeProvider,
        serializer,
        configuration,
        logger,
        tracer
    )
    {
        _configuration = configuration;
        _disposable = Disposable.Box(tracer);
        Socket.ConnectionLost += () => ConnectionLost.Invoke();
        Socket.ConnectionRestored += async () =>
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
        await Task.WhenAll(
            WaitConnectionReadyAsync(ct),
            Socket.ConnectAsync(_configuration.Uri, ct)
        );
        this.Trace("done");
    }

    public Task DisconnectAsync() =>
        Socket.DisconnectAsync();


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
        await Socket.DisconnectAsync();
        this.Trace("dispose socket");
        await Socket.DisposeAsync();
        this.Trace("done");

        _isDisposed = true;
    }

    private void HandleConnectionReady()
    {
        _connectionTcs.SetResult(null);
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