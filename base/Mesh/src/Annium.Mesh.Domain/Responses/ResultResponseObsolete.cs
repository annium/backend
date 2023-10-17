using System;
using Annium.Architecture.Base;
using Annium.Data.Operations;

namespace Annium.Mesh.Domain.Responses;

[Obsolete("Old messaging model")]
public sealed class ResultResponseObsolete : ResponseBaseObsolete
{
    public IStatusResult<OperationStatus> Result { get; }

    public ResultResponseObsolete(
        Guid rid,
        IStatusResult<OperationStatus> result
    ) : base(rid)
    {
        Result = result;
    }
}