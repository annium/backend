using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Annium.Infrastructure.WebSockets.Domain.Models;
using Annium.Infrastructure.WebSockets.Domain.Requests;
using Annium.Infrastructure.WebSockets.Server.Internal.Models;

namespace Annium.Infrastructure.WebSockets.Server.Internal.Handlers.Subscriptions
{
    internal class SubscriptionContextStore<TInit, TMessage, TState> : IDisposable
        where TInit : SubscriptionInitRequestBase
        where TState : ConnectionStateBase
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

        public void Save(SubscriptionContext<TInit, TMessage, TState> context)
        {
            lock (_contexts) _contexts.Add(context);
        }

        public async Task<bool> TryRemove(Guid subscriptionId)
        {
            SubscriptionContext<TInit, TMessage, TState> context;
            lock (_contexts)
            {
                context = _contexts.SingleOrDefault(x => x.SubscriptionId == subscriptionId)!;
                if (context is null!)
                    return false;

                _contexts.Remove(context);
            }

            await context.DisposeAsync();

            return true;
        }

        public void Dispose()
        {
            _connectionTracker.OnRelease -= Cleanup;
        }

        private async Task Cleanup(Guid connectionId)
        {
            var contexts = new List<SubscriptionContext<TInit, TMessage, TState>>();
            lock (_contexts)
            {
                foreach (var context in _contexts.ToArray())
                    if (context.ConnectionId == connectionId)
                    {
                        contexts.Add(context);
                        _contexts.Remove(context);
                    }
            }

            await Task.WhenAll(contexts.Select(async x => await x.DisposeAsync()));
        }
    }
}