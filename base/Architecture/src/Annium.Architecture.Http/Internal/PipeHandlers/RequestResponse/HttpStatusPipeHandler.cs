using System.Net;
using Annium.Architecture.Base;
using Annium.Core.Mediator;
using Annium.Data.Operations;

namespace Annium.Architecture.Http.Internal.PipeHandlers.RequestResponse
{
    internal class HttpStatusPipeHandler<TRequest, TResponse> : HttpStatusPipeHandlerBase<TRequest, IStatusResult<OperationStatus, TResponse>, IStatusResult<HttpStatusCode, TResponse>>, IPipeRequestHandler<TRequest, TRequest, IStatusResult<OperationStatus, TResponse>, IStatusResult<HttpStatusCode, TResponse>>
    {
        protected override IStatusResult<HttpStatusCode, TResponse> GetResponse(IStatusResult<OperationStatus, TResponse> response)
        {
            return Result.Status(MapToStatusCode(response.Status), response.Data).Join(response);
        }
    }
}