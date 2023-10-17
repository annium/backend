using System;

namespace Annium.Mesh.Domain;

public sealed record SubscriptionCancel
{
    public Guid Id { get; init; }
}