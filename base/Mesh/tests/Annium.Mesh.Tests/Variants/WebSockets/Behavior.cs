using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection.Container;
using Annium.Core.DependencyInjection.Extensions;
using Annium.Logging;
using Annium.Mesh.Server.Web;
using Annium.Mesh.Tests.System.Client;
using Annium.Mesh.Transport.WebSockets;
using Annium.Net.Servers.Web;

namespace Annium.Mesh.Tests.Variants.WebSockets;

/// <summary>
/// Test behavior implementation for WebSocket-based mesh transport, configuring and running a WebSocket server.
/// </summary>
public class Behavior : IBehavior, ILogSubject
{
    /// <summary>
    /// Gets the logger for this behavior.
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// Base port number for WebSocket server instances, incremented for each new instance.
    /// </summary>
    private static int _basePort = 20000;

    /// <summary>
    /// Registers services required for WebSocket-based mesh transport and server functionality.
    /// </summary>
    /// <param name="container">The service container to register services in.</param>
    public static void Register(IServiceContainer container)
    {
        container.Add(new TransportConfiguration(Interlocked.Increment(ref _basePort))).AsSelf().Singleton();

        container.AddMeshWebSocketsClientTransport(sp => new ClientTransportConfiguration
        {
            Uri = new Uri($"ws://127.0.0.1:{sp.Resolve<TransportConfiguration>().Port}"),
        });
        container.AddMeshWebSocketsServerTransport(_ => new ServerTransportConfiguration());
        container.AddWebServerMeshHandler();

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
    /// Runs the WebSocket-based mesh server asynchronously for the duration of the test.
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
    /// Configuration record containing the port number for the WebSocket transport.
    /// </summary>
    /// <param name="Port">The port number for the WebSocket server.</param>
    public record TransportConfiguration(int Port);
}
