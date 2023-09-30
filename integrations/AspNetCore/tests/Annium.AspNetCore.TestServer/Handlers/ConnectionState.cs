using Annium.Logging;
using Annium.Mesh.Server.Models;

namespace Annium.AspNetCore.TestServer.Handlers;

public class ConnectionState : ConnectionStateBase
{
    public ConnectionState(ILogger logger) : base(logger)
    {
    }
}