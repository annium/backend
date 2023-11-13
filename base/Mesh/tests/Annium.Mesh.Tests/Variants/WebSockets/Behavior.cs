using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Logging;
using Annium.Mesh.Server.Web;
using Annium.Mesh.Tests.System.Client;
using Annium.Mesh.Transport.WebSockets;
using Annium.Net.Servers.Web;

namespace Annium.Mesh.Tests.Variants.WebSockets;

public class Behavior : IBehavior, ILogSubject
{
    public ILogger Logger { get; }
    private static int _basePort = 20000;

    public static void Register(IServiceContainer container)
    {
        container.Add(new TransportConfiguration(Interlocked.Increment(ref _basePort))).AsSelf().Singleton();

        container.AddMeshWebSocketsClientTransport(
            sp =>
                new ClientTransportConfiguration
                {
                    Uri = new Uri($"ws://127.0.0.1:{sp.Resolve<TransportConfiguration>().Port}")
                }
        );
        container.AddMeshWebSocketsServerTransport(_ => new ServerTransportConfiguration());
        container.AddWebServerMeshHandler();

        container.AddTestServerClient(x => x.WithResponseTimeout(6000));
    }

    private readonly IServiceProvider _sp;
    private readonly TransportConfiguration _config;

    public Behavior(IServiceProvider sp, TransportConfiguration config, ILogger logger)
    {
        Logger = logger;
        _sp = sp;
        _config = config;
    }

    public Task RunServer(CancellationToken ct)
    {
        this.Trace("start, bind server to port {port}", _config.Port);
        var server = ServerBuilder.New(_sp, _config.Port).WithMeshHandler().Build();

        this.Trace("run server");
        var runTask = server.RunAsync(ct);

        this.Trace("done, server running");

        return runTask;
    }

    public record TransportConfiguration(int Port);
}
