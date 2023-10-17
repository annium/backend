using System;
using Annium.Architecture.Base;
using Annium.Data.Operations;

namespace Annium.Mesh.Domain.Responses;

[Obsolete("Old messaging model")]
public class ResultResponseObsolete<T> : ResponseBaseObsolete
{
    public IStatusResult<OperationStatus, T> Result { get; }

    public ResultResponseObsolete(
        Guid rid,
        IStatusResult<OperationStatus, T> result
    ) : base(rid)
    {
        Result = result;
    }
}