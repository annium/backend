using Annium.Infrastructure.WebSockets.Server.Models;
using Annium.Logging;

namespace Demo.Infrastructure.WebSockets.Server.Handlers;

internal class ConnectionState : ConnectionStateBase
{
    public int Value { get; set; }

    public ConnectionState(ILogger logger) : base(logger)
    {
    }
}