using Annium.Core.Runtime.Types;

namespace Annium.Mesh.Domain.Responses;

public abstract class AbstractResponseBase
{
    [ResolutionId]
    public string Tid => GetType().GetIdString();
}