using System;

namespace Annium.Mesh.Domain.Responses;

public abstract class ResponseBase : AbstractResponseBase
{
    public Guid Rid { get; }

    public ResponseBase(Guid rid)
    {
        Rid = rid;
    }
}