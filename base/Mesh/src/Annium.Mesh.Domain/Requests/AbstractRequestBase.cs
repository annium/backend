using System;
using Annium.Core.Runtime.Types;

namespace Annium.Mesh.Domain.Requests;

public abstract class AbstractRequestBase
{
    public Guid Rid { get; set; } = Guid.NewGuid();

    [ResolutionId]
    public string Tid => GetType().GetIdString();
}