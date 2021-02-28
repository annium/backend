using System;
using System.Threading;
using Annium.Infrastructure.WebSockets.Domain.Models;
using Annium.Infrastructure.WebSockets.Domain.Responses;

namespace Annium.Infrastructure.WebSockets.Server.Internal.Models
{
    internal class BroadcastContext<TMessage> : IBroadcastContext<TMessage>
        where TMessage : NotificationBase
    {
        public CancellationToken Token { get; }
        private readonly Action<object> _send;

        public BroadcastContext(
            Action<object> send,
            CancellationToken ct
        )
        {
            _send = send;
            Token = ct;
        }

        public void Send(TMessage message) => _send(message);
    }
}