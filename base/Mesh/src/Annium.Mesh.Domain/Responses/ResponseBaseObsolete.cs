using System;

namespace Annium.Mesh.Domain.Responses;

[Obsolete("Old messaging model")]
public abstract class ResponseBaseObsolete : AbstractResponseBaseObsolete
{
    public Guid Rid { get; }

    public ResponseBaseObsolete(Guid rid)
    {
        Rid = rid;
    }
}