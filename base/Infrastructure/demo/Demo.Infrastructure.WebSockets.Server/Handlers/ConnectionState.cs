using Annium.Infrastructure.WebSockets.Server.Models;

namespace Demo.Infrastructure.WebSockets.Server.Handlers
{
    internal class ConnectionState : ConnectionStateBase
    {
        public int Value { get; set; }
    }
}