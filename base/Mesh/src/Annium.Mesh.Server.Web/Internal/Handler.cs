using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Logging;
using Annium.Mesh.Transport.Abstractions;
using Annium.Net.Servers.Web;

namespace Annium.Mesh.Server.Web.Internal;

public class Handler : IWebSocketHandler, ILogSubject
{
    public ILogger Logger { get; }
    private readonly IServerConnectionFactory<WebSocket> _connectionFactory;
    private readonly ICoordinator _coordinator;

    public Handler(IServiceProvider sp)
    {
        Logger = sp.Resolve<ILogger>();
        _connectionFactory = sp.Resolve<IServerConnectionFactory<WebSocket>>();
        _coordinator = sp.Resolve<ICoordinator>();
    }

    public async Task HandleAsync(HttpListenerWebSocketContext ctx, CancellationToken ct)
    {
        try
        {
            this.Trace("start");

            this.Trace("create connection");
            var connection = await _connectionFactory.CreateAsync(ctx.WebSocket);

            this.Trace("handle connection");
            await _coordinator.HandleAsync(connection);

            this.Trace("done");
        }
        catch (Exception ex)
        {
            this.Error(ex);
        }
    }
}
