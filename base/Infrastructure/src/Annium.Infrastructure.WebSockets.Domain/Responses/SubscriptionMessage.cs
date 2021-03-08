using System;

namespace Annium.Infrastructure.WebSockets.Domain.Responses
{
    public sealed class SubscriptionMessage<T> : AbstractResponseBase
    {
        public Guid SubscriptionId { get; }
        public T Message { get; }

        public SubscriptionMessage(Guid subscriptionId, T message)
        {
            SubscriptionId = subscriptionId;
            Message = message;
        }
    }
}