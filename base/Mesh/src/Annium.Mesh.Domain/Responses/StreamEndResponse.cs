using System;

namespace Annium.Mesh.Domain.Responses;

public sealed class StreamEndResponse : ResponseBase
{
    public StreamEndResponse(
        Guid rid
    ) : base(rid)
    {
    }
}