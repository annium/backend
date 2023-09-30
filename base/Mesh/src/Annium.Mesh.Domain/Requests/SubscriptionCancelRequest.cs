using System;
using System.Text.Json.Serialization;

namespace Annium.Mesh.Domain.Requests;

public sealed class SubscriptionCancelRequest : AbstractRequestBase
{
    public static SubscriptionCancelRequest New(Guid subscriptionId) => new()
    {
        SubscriptionId = subscriptionId
    };

    [JsonInclude]
    public Guid SubscriptionId { get; private set; }
}