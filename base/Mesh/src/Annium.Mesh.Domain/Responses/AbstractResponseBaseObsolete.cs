using System;
using Annium.Core.Runtime.Types;

namespace Annium.Mesh.Domain.Responses;

[Obsolete("Old messaging model")]
public abstract class AbstractResponseBaseObsolete
{
    [ResolutionId]
    public string Tid => GetType().GetIdString();
}