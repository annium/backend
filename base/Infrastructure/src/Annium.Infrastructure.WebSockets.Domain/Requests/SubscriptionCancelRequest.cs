using System;

namespace Annium.Infrastructure.WebSockets.Domain.Requests
{
    public sealed record SubscriptionCancelRequest : RequestBase
    {
        public static SubscriptionCancelRequest New(Guid subscriptionId) => new()
        {
            SubscriptionId = subscriptionId
        };

        public Guid SubscriptionId { get; init; }
    }
}