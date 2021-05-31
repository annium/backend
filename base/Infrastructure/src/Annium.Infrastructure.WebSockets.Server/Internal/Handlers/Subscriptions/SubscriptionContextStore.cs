using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Annium.Core.Internal;
using Annium.Infrastructure.WebSockets.Server.Internal.Models;

namespace Annium.Infrastructure.WebSockets.Server.Internal.Handlers.Subscriptions
{
    internal class SubscriptionContextStore : IConnectionBoundStore
    {
        private readonly List<ISubscriptionContext> _contexts = new();

        public void Save(ISubscriptionContext context)
        {
            lock (_contexts) _contexts.Add(context);
        }

        public async Task<bool> TryRemove(Guid subscriptionId)
        {
            this.Trace(() => $"subscription {subscriptionId} - init");
            ISubscriptionContext context;
            lock (_contexts)
            {
                context = _contexts.SingleOrDefault(x => x.SubscriptionId == subscriptionId)!;
                if (context is null!)
                {
                    this.Trace(() => $"subscription {subscriptionId} - context missing");
                    return false;
                }

                _contexts.Remove(context);
            }

            await context.DisposeAsync();

            return true;
        }

        public async Task Cleanup(Guid connectionId)
        {
            var contexts = new List<ISubscriptionContext>();
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