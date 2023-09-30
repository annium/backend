using System;
using Annium.Architecture.Base;
using Annium.Data.Operations;

namespace Annium.Mesh.Domain.Responses;

public sealed class StreamHeadResponse<T> : ResponseBase
{
    public IStatusResult<OperationStatus, T> Result { get; }

    public StreamHeadResponse(
        Guid rid,
        IStatusResult<OperationStatus, T> result
    ) : base(rid)
    {
        Result = result;
    }
}