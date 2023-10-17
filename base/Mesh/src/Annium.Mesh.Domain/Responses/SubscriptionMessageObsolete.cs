using System;

namespace Annium.Mesh.Domain.Responses;

[Obsolete("Old messaging model")]
public sealed class SubscriptionMessageObsolete<T> : AbstractResponseBaseObsolete
{
    public Guid SubscriptionId { get; }
    public T Message { get; }

    public SubscriptionMessageObsolete(Guid subscriptionId, T message)
    {
        SubscriptionId = subscriptionId;
        Message = message;
    }
}