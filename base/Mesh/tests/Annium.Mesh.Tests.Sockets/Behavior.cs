using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Logging;
using Annium.Mesh.Server.Sockets;
using Annium.Mesh.Tests.Base;
using Annium.Mesh.Tests.System.Client;
using Annium.Mesh.Transport.Sockets;
using Annium.Net.Servers.Sockets;

namespace Annium.Mesh.Tests.Sockets;

public class Behavior : IBehavior, ILogSubject
{
    public ILogger Logger { get; }
    private static int _basePort = 10000;

    public static void Register(IServiceContainer container)
    {
        container.Add(new TransportConfiguration(Interlocked.Increment(ref _basePort))).AsSelf().Singleton();

        container.AddMeshSocketsClientTransport(sp => new ClientTransportConfiguration
        {
            Endpoint = new IPEndPoint(IPAddress.Loopback, sp.Resolve<TransportConfiguration>().Port)
        });
        container.AddMeshSocketsServerTransport(_ => new ServerTransportConfiguration());
        container.AddSocketServerMeshHandler();

        container.AddTestServerClient(x => x.WithResponseTimeout(6000));
    }

    private readonly IServiceProvider _sp;
    private readonly TransportConfiguration _config;

    public Behavior(
        IServiceProvider sp,
        TransportConfiguration config,
        ILogger logger
    )
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