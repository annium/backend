using System;
using Annium.Core.Runtime.Types;

namespace Annium.Mesh.Domain.Requests;

[Obsolete("Old messaging model")]
public abstract class AbstractRequestBaseObsolete
{
    public Guid Rid { get; set; } = Guid.NewGuid();

    [ResolutionId]
    public string Tid => GetType().GetIdString();
}