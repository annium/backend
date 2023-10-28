using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Mesh.Transport.Abstractions;
using Annium.Net.Servers.Sockets;

namespace Annium.Mesh.Server.Sockets.Internal;

internal class Handler : IHandler, ILogSubject
{
    public ILogger Logger { get; }
    private readonly IServerConnectionFactory<Socket> _connectionFactory;
    private readonly ICoordinator _coordinator;

    public Handler(IServerConnectionFactory<Socket> connectionFactory, ICoordinator coordinator, ILogger logger)
    {
        Logger = logger;
        _connectionFactory = connectionFactory;
        _coordinator = coordinator;
    }

    public async Task HandleAsync(Socket socket, CancellationToken ct)
    {
        try
        {
            this.Trace("start");

            this.Trace("create connection");
            var connection = await _connectionFactory.CreateAsync(socket);

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
