using System;
using Annium.Infrastructure.WebSockets.Domain.Models;

namespace Annium.AspNetCore.TestServer.Handlers
{
    public class ConnectionState : ConnectionStateBase
    {
        public ConnectionState(Guid connectionId) : base(connectionId)
        {
        }
    }
}