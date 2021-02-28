using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Infrastructure.WebSockets.Domain.Responses;
using Annium.Infrastructure.WebSockets.Server.Handlers;
using Annium.Infrastructure.WebSockets.Server.Internal.Models;

namespace Annium.Infrastructure.WebSockets.Server.Internal.Handlers
{
    internal class BroadcasterRunner<TMessage> : IBroadcasterRunner
        where TMessage : NotificationBase
    {
        private readonly IBroadcaster<TMessage> _broadcaster;

        public BroadcasterRunner(
            IBroadcaster<TMessage> broadcaster
        )
        {
            _broadcaster = broadcaster;
        }

        public Task Run(Action<object> send, CancellationToken ct)
        {
            var ctx = new BroadcastContext<TMessage>(send, ct);

            return _broadcaster.Run(ctx);
        }
    }

    internal interface IBroadcasterRunner
    {
        Task Run(Action<object> send, CancellationToken ct);
    }
}