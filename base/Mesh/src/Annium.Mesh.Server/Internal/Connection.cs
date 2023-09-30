using System;
using Annium.Logging;
using Annium.Net.WebSockets;

namespace Annium.Mesh.Server.Internal;

internal class Connection : IDisposable, ILogSubject
{
    public Guid Id { get; }
    public ISendingReceivingWebSocket Socket => _socket;
    public ILogger Logger { get; }
    private readonly IServerWebSocket _socket;

    public Connection(
        Guid id,
        IServerWebSocket socket,
        ILogger logger
    )
    {
        Id = id;
        _socket = socket;
        Logger = logger;
    }

    public void Dispose()
    {
        this.Trace("start");

        this.Trace("disconnect socket");
        _socket.Disconnect();

        this.Trace("done");
    }
}