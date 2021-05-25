using System;
using System.Text.Json.Serialization;

namespace Annium.Infrastructure.WebSockets.Domain.Requests
{
    public abstract class SubscriptionInitRequestBase : AbstractRequestBase
    {
        [JsonInclude]
        public Guid SubscriptionId { get; private set; }

        public void SetId()
        {
            if (SubscriptionId != Guid.Empty)
                throw new InvalidOperationException("Subscription ID is already set");

            SubscriptionId = Guid.NewGuid();
        }
    }
}