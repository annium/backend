using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Annium.Infrastructure.WebSockets.Server.Internal.Models;
using Annium.Logging.Abstractions;

namespace Annium.Infrastructure.WebSockets.Server.Internal.Handlers.Subscriptions;

internal class SubscriptionContextStore : IConnectionBoundStore, ILogSubject<SubscriptionContextStore>
{
    public ILogger<SubscriptionContextStore> Logger { get; }
    private readonly List<ISubscriptionContext> _contexts = new();

    public SubscriptionContextStore(
        ILogger<SubscriptionContextStore> logger
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
        this.Log().Trace($"subscription {subscriptionId} - init");
        lock (_contexts)
        {
            var context = _contexts.SingleOrDefault(x => x.SubscriptionId == subscriptionId);
            if (context is null)
            {
                this.Log().Trace($"subscription {subscriptionId} - context missing");
                return false;
            }

            this.Log().Trace($"subscription {subscriptionId} - cancel context");
            _contexts.Remove(context);
            context.Cancel();

            return true;
        }
    }

    public Task Cleanup(Guid connectionId)
    {
        lock (_contexts)
            foreach (var context in _contexts.Where(x => x.ConnectionId == connectionId).ToArray())
            {
                _contexts.Remove(context);
                context.Cancel();
            }

        return Task.CompletedTask;
    }
}