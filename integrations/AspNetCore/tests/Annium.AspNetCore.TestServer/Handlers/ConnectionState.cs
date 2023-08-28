using Annium.Debug;
using Annium.Infrastructure.WebSockets.Server.Models;

namespace Annium.AspNetCore.TestServer.Handlers;

public class ConnectionState : ConnectionStateBase
{
    public ConnectionState(ITracer tracer) : base(tracer)
    {
    }
}