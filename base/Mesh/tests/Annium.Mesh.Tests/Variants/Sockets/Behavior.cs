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
    /// Registers services required for socket-based mesh transport and server functionality.
    /// </summary>
    /// <param name="container">The service container to register services in.</param>
    public static void Register(IServiceContainer container)
    {
        container.Add(static sp => ServerBuilder.New(sp).WithMeshHandler().Start().NotNull()).AsSelf().Singleton();

        container.AddSocketsDefaultConnectionMonitorFactory();

        container.AddMeshSocketsClientTransport(sp => new ClientTransportConfiguration
        {
            Uri = sp.Resolve<IServer>().TcpUri(),
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
    /// Gets the logger for this behavior.
    /// </summary>
    public ILogger Logger { get; }

    private readonly IServer _server;

    /// <summary>
    /// Initializes a new instance of the <see cref="Behavior"/> class with the specified dependencies.
    /// </summary>
    /// <param name="server">Server, used by current test behavior</param>
    /// <param name="logger">The logger for this behavior.</param>
    public Behavior(IServer server, ILogger logger)
    {
        Logger = logger;
        _server = server;
    }

    public ValueTask InitializeAsync()
    {
        return ValueTask.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        return _server.DisposeAsync();
    }
}
