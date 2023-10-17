using System;
using System.Text.Json.Serialization;

namespace Annium.Mesh.Domain.Requests;

[Obsolete("Old messaging model")]
public sealed class SubscriptionCancelRequestObsolete : AbstractRequestBaseObsolete
{
    public static SubscriptionCancelRequestObsolete New(Guid subscriptionId) => new()
    {
        SubscriptionId = subscriptionId
    };

    [JsonInclude]
    public Guid SubscriptionId { get; private set; }
}