using System;
using Annium.Architecture.Base;
using Annium.Data.Operations;

namespace Annium.Infrastructure.WebSockets.Domain.Responses;

public class ResultResponse<T> : ResponseBase
{
    public IStatusResult<OperationStatus, T> Result { get; }

    public ResultResponse(
        Guid rid,
        IStatusResult<OperationStatus, T> result
    ) : base(rid)
    {
        Result = result;
    }
}