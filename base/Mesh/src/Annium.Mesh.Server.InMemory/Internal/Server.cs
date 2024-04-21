using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Execution.Background;
using Annium.Logging;
using Annium.Mesh.Transport.Abstractions;
using Annium.Mesh.Transport.InMemory;
using Annium.Threading;

namespace Annium.Mesh.Server.InMemory.Internal;

internal class Server : IServer, ILogSubject
{
    public ILogger Logger { get; }
    private readonly IConnectionHub _hub;
    private readonly ICoordinator _coordinator;
    private readonly IExecutor _executor;
    private int _isListening;

    public Server(IConnectionHub hub, ICoordinator coordinator, ILogger logger)
    {
        _hub = hub;
        _coordinator = coordinator;
        Logger = logger;
        _executor = Executor.Parallel<Server>(Logger);
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        this.Trace("start");

        if (Interlocked.CompareExchange(ref _isListening, 1, 0) == 1)
            throw new InvalidOperationException("Server is already started");

        this.Trace("start executor");
        _executor.Start(ct);

        _hub.OnConnection += OnConnection;

        await ct;

        _hub.OnConnection -= OnConnection;

        // when cancelled - await connections processing and stop listener
        this.Trace("dispose executor");
        await _executor.DisposeAsync().ConfigureAwait(false);
    }

    private void OnConnection(IServerConnection connection)
    {
        // try schedule socket handling
        if (_executor.Schedule(HandleConnection(connection)))
        {
            this.Trace("connection handle scheduled");
        }
        else
            this.Trace("connection handle skipped (server is already stopping)");
    }

    private Func<ValueTask> HandleConnection(IServerConnection connection) =>
        async () =>
        {
            try
            {
                this.Trace("start");

                this.Trace("handle connection");
                await _coordinator.HandleAsync(connection);

                this.Trace("done");
            }
            catch (Exception ex)
            {
                this.Error(ex);
            }
        };
}
