using System;
using Annium.Architecture.Base;
using Annium.Data.Operations;

namespace Annium.Infrastructure.WebSockets.Domain.Responses
{
    public sealed record ResultResponse : ResponseBase
    {
        public IStatusResult<OperationStatus> Result { get; }

        public ResultResponse(
            Guid rid,
            IStatusResult<OperationStatus> result
        ) : base(rid)
        {
            Result = result;
        }
    }
}