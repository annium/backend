using System;

namespace Annium.Mesh.Domain;

public abstract class SubscriptionInitBase
{
    public Guid Id { get; init; } = Guid.NewGuid();
}