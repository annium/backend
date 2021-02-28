using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Core.Mediator;
using Annium.Data.Operations;
using Annium.Infrastructure.WebSockets.Domain.Models;
using Annium.Infrastructure.WebSockets.Domain.Requests;
using Annium.Infrastructure.WebSockets.Domain.Responses;
using Annium.Infrastructure.WebSockets.Server.Internal.Responses;

namespace Annium.Infrastructure.WebSockets.Server.Internal.Handlers.Requests
{
    internal class RequestResponseHandler<TRequest, TResponse> :
        IPipeRequestHandler<IRequestContext<TRequest>, IRequestContext<TRequest>, IStatusResult<OperationStatus, TResponse>, ResultResponse<TResponse>>
        where TRequest : RequestBase
    {
        public async Task<ResultResponse<TResponse>> HandleAsync(
            IRequestContext<TRequest> request,
            CancellationToken ct,
            Func<IRequestContext<TRequest>, Task<IStatusResult<OperationStatus, TResponse>>> next
        )
        {
            var result = await next(request);

            return Response.Result(request.Request.Rid, result);
        }
    }
}