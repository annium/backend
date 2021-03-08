using System;
using Annium.Architecture.Base;
using Annium.Data.Operations;

namespace Annium.Infrastructure.WebSockets.Domain.Responses
{
    public sealed class ResultResponse : ResponseBase
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