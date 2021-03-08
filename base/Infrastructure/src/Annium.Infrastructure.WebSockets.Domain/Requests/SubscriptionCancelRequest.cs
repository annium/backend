using System;

namespace Annium.Infrastructure.WebSockets.Domain.Requests
{
    public sealed class SubscriptionCancelRequest : RequestBase
    {
        public static SubscriptionCancelRequest New(Guid subscriptionId) => new SubscriptionCancelRequest
        {
            SubscriptionId = subscriptionId
        };

        public Guid SubscriptionId { get; set; }
    }
}