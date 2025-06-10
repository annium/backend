using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Execution.Background;
using Annium.Logging;
using Annium.Mesh.Transport.Abstractions;
using Annium.Mesh.Transport.InMemory;
using Annium.Threading;

namespace Annium.Mesh.Server.InMemory.Internal;

/// <summary>
/// In-memory implementation of a mesh server that handles client connections through a connection hub.
/// </summary>
internal class Server : IServer, ILogSubject
{
    /// <summary>
    /// Gets the logger for this server instance.
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// The connection hub for managing in-memory connections.
    /// </summary>
    private readonly IConnectionHub _hub;

    /// <summary>
    /// The coordinator for handling connection processing.
    /// </summary>
    private readonly ICoordinator _coordinator;

    /// <summary>
    /// The executor for parallel processing of connections.
    /// </summary>
    private readonly IExecutor _executor;

    /// <summary>
    /// Flag indicating whether the server is currently listening for connections.
    /// </summary>
    private int _isListening;

    /// <summary>
    /// Initializes a new instance of the <see cref="Server"/> class.
    /// </summary>
    /// <param name="hub">The connection hub for managing in-memory connections.</param>
    /// <param name="coordinator">The coordinator for handling connection processing.</param>
    /// <param name="logger">The logger for this server instance.</param>
    public Server(IConnectionHub hub, ICoordinator coordinator, ILogger logger)
    {
        _hub = hub;
        _coordinator = coordinator;
        Logger = logger;
        _executor = Executor.Parallel<Server>(Logger);
    }

    /// <summary>
    /// Runs the server asynchronously, listening for incoming connections until cancellation is requested.
    /// </summary>
    /// <param name="ct">The cancellation token to stop the server.</param>
    /// <returns>A task that represents the asynchronous server operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the server is already started.</exception>
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

    /// <summary>
    /// Handles a new connection from the connection hub by scheduling it for processing.
    /// </summary>
    /// <param name="connection">The server connection to handle.</param>
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

    /// <summary>
    /// Creates a handler function for processing a server connection through the coordinator.
    /// </summary>
    /// <param name="connection">The server connection to handle.</param>
    /// <returns>A function that asynchronously handles the connection.</returns>
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
