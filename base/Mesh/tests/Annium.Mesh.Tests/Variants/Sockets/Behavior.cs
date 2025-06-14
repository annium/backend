using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Logging;
using Annium.Mesh.Server.Sockets;
using Annium.Mesh.Tests.System.Client;
using Annium.Mesh.Transport.Sockets;
using Annium.Net.Servers.Sockets;
using Annium.Net.Sockets;

namespace Annium.Mesh.Tests.Variants.Sockets;

/// <summary>
/// Test behavior implementation for socket-based mesh transport, configuring and running a TCP socket server.
/// </summary>
public class Behavior : IBehavior, ILogSubject
{
    /// <summary>
    /// Gets the logger for this behavior.
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// Base port number for socket server instances, incremented for each new instance.
    /// </summary>
    private static int _basePort = 10000;

    /// <summary>
    /// Registers services required for socket-based mesh transport and server functionality.
    /// </summary>
    /// <param name="container">The service container to register services in.</param>
    public static void Register(IServiceContainer container)
    {
        container.Add(new TransportConfiguration(Interlocked.Increment(ref _basePort))).AsSelf().Singleton();
        container.AddSocketsDefaultConnectionMonitorFactory();

        container.AddMeshSocketsClientTransport(sp => new ClientTransportConfiguration
        {
            Uri = new Uri($"tcp://127.0.0.1:{sp.Resolve<TransportConfiguration>().Port}"),
            ConnectionMonitor = new ConnectionMonitorOptions { Factory = sp.Resolve<IConnectionMonitorFactory>() },
        });
        container.AddMeshSocketsServerTransport(sp => new ServerTransportConfiguration
        {
            ConnectionMonitor = new ConnectionMonitorOptions { Factory = sp.Resolve<IConnectionMonitorFactory>() },
        });
        container.AddSocketServerMeshHandler();

        container.AddTestServerClient(x => x.WithResponseTimeout(6000));
    }

    /// <summary>
    /// The service provider for creating server instances.
    /// </summary>
    private readonly IServiceProvider _sp;

    /// <summary>
    /// The transport configuration containing server port information.
    /// </summary>
    private readonly TransportConfiguration _config;

    /// <summary>
    /// Initializes a new instance of the <see cref="Behavior"/> class with the specified dependencies.
    /// </summary>
    /// <param name="sp">The service provider for creating server instances.</param>
    /// <param name="config">The transport configuration containing server port information.</param>
    /// <param name="logger">The logger for this behavior.</param>
    public Behavior(IServiceProvider sp, TransportConfiguration config, ILogger logger)
    {
        Logger = logger;
        _sp = sp;
        _config = config;
    }

    /// <summary>
    /// Runs the socket-based mesh server asynchronously for the duration of the test.
    /// </summary>
    /// <param name="ct">The cancellation token to stop the server.</param>
    /// <returns>A task representing the asynchronous server operation.</returns>
    public Task RunServerAsync(CancellationToken ct)
    {
        this.Trace("start, bind server to port {port}", _config.Port);
        var server = ServerBuilder.New(_sp, _config.Port).WithMeshHandler().Build();

        this.Trace("run server");
        var runTask = server.RunAsync(ct);

        this.Trace("done, server running");

        return runTask;
    }

    /// <summary>
    /// Configuration record containing the port number for the socket transport.
    /// </summary>
    /// <param name="Port">The port number for the socket server.</param>
    public record TransportConfiguration(int Port);
}
