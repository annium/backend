using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Mesh.Server.Internal.Models;

namespace Annium.Mesh.Server.Internal.Handlers.Subscriptions;

internal class SubscriptionContextStore : IConnectionBoundStore, ILogSubject
{
    public ILogger Logger { get; }
    private readonly List<ISubscriptionContext> _contexts = new();

    public SubscriptionContextStore(
        ILogger logger
    )
    {
        Logger = logger;
    }

    public void Save(ISubscriptionContext context)
    {
        lock (_contexts) _contexts.Add(context);
    }

    public bool TryCancel(Guid subscriptionId)
    {
        this.Trace("subscription {subscriptionId} - init", subscriptionId);
        lock (_contexts)
        {
            var context = _contexts.SingleOrDefault(x => x.SubscriptionId == subscriptionId);
            if (context is null)
            {
                this.Trace("subscription {subscriptionId} - context missing", subscriptionId);
                return false;
            }

            this.Trace("subscription {subscriptionId} - cancel context", subscriptionId);
            _contexts.Remove(context);
            context.Cancel();

            return true;
        }
    }

    public Task Cleanup(Guid connectionId)
    {
        this.Trace("cleanup {connectionId} subscriptions - start", connectionId);
        lock (_contexts)
            foreach (var context in _contexts.Where(x => x.ConnectionId == connectionId).ToArray())
            {
                this.Trace("cancel {subscriptionId} subscription", context.SubscriptionId);
                _contexts.Remove(context);
                context.Cancel();
            }

        this.Trace("cleanup {connectionId} subscriptions - done", connectionId);

        return Task.CompletedTask;
    }
}