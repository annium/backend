using System;
using Annium.Infrastructure.WebSockets.Domain.Models;

namespace Demo.Infrastructure.WebSockets.Server.Handlers
{
    internal class State : ConnectionState
    {
        public int Value { get; set; }
        public State(Guid connectionId) : base(connectionId)
        {
        }
    }
}