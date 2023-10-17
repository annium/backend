using System;

namespace Annium.Mesh.Domain;

public abstract class RequestBase
{
    public Guid Id { get; init; } = Guid.NewGuid();
}