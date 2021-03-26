using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Infrastructure.WebSockets.Domain.Models;
using Annium.Infrastructure.WebSockets.Domain.Requests;
using Annium.Infrastructure.WebSockets.Server.Internal.Models;

namespace Annium.Infrastructure.WebSockets.Server.Internal.Handlers.Subscriptions
{
    internal class SubscriptionContextStore<TInit, TMessage, TState> : IDisposable
        where TInit : SubscriptionInitRequestBase
        where TState : ConnectionState
    {
        private readonly ConnectionTracker _connectionTracker;
        private readonly List<SubscriptionContext<TInit, TMessage, TState>> _contexts = new();

        public SubscriptionContextStore(
            ConnectionTracker connectionTracker
        )
        {
            _connectionTracker = connectionTracker;
            _connectionTracker.OnRelease += Cleanup;
        }

        public void Save(SubscriptionContext<TInit, TMessage, TState> ctx)
        {
            lock (_contexts) _contexts.Add(ctx);
        }

        public bool TryRemove(Guid subscriptionId)
        {
            lock (_contexts)
            {
                var ctx = _contexts.SingleOrDefault(x => x.SubscriptionId == subscriptionId)!;
                if (ctx is null!)
                    return false;

                _contexts.Remove(ctx);
                ctx.Cancel();

                return true;
            }
        }

        public void Dispose()
        {
            _connectionTracker.OnRelease -= Cleanup;
        }

        private void Cleanup(Guid connectionId)
        {
            lock (_contexts)
            {
                var contexts = _contexts
                    .Where(x => x.ConnectionId == connectionId)
                    .ToArray();
                foreach (var context in contexts)
                {
                    _contexts.Remove(context);
                    context.Cancel();
                }
            }
        }
    }
}