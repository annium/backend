using Annium.Logging;
using Annium.Mesh.Server.Models;

namespace Demo.Mesh.Server.Handlers;

internal class ConnectionState : ConnectionStateBase
{
    public int Value { get; set; }

    public ConnectionState(ILogger logger) : base(logger)
    {
    }
}