using Annium.Infrastructure.WebSockets.Server.Models;
using Annium.Logging;

namespace Annium.AspNetCore.TestServer.Handlers;

public class ConnectionState : ConnectionStateBase
{
    public ConnectionState(ILogger logger) : base(logger)
    {
    }
}