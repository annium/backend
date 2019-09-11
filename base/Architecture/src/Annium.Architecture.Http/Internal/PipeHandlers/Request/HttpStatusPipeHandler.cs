using System.Net;
using Annium.Architecture.Base;
using Annium.Core.Mediator;
using Annium.Data.Operations;

namespace Annium.Architecture.Http.Internal.PipeHandlers.Request
{
    internal class HttpStatusPipeHandler<TRequest> : HttpStatusPipeHandlerBase<TRequest, IStatusResult<OperationStatus>, IStatusResult<HttpStatusCode>>, IPipeRequestHandler<TRequest, TRequest, IStatusResult<OperationStatus>, IStatusResult<HttpStatusCode>>
    {
        protected override IStatusResult<HttpStatusCode> GetResponse(IStatusResult<OperationStatus> response)
        {
            return Result.Status(MapToStatusCode(response.Status)).Join(response);
        }
    }
}