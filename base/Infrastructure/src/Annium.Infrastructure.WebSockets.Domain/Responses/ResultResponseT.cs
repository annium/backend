using System;
using Annium.Architecture.Base;
using Annium.Data.Operations;

namespace Annium.Infrastructure.WebSockets.Domain.Responses
{
    public record ResultResponse<T> : ResponseBase
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
}