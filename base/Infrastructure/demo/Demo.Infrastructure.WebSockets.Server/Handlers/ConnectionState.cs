using Annium.Infrastructure.WebSockets.Domain.Models;

namespace Demo.Infrastructure.WebSockets.Server.Handlers
{
    internal class ConnectionState : ConnectionStateBase
    {
        public int Value { get; set; }
    }
}