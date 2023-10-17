using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Core.Mediator;
using Annium.Data.Operations;
using Annium.Mesh.Domain.Requests;
using Annium.Mesh.Domain.Responses;
using Annium.Mesh.Server.Internal.Responses;
using Annium.Mesh.Server.Models;

namespace Annium.Mesh.Server.Internal.Handlers.Requests;

internal class RequestResponseHandler<TRequest, TResponse> :
    IPipeRequestHandler<IRequestContext<TRequest>, IRequestContext<TRequest>, IStatusResult<OperationStatus, TResponse>, ResultResponse<TResponse>>
    where TRequest : RequestBase
{
    public async Task<ResultResponse<TResponse>> HandleAsync(
        IRequestContext<TRequest> request,
        CancellationToken ct,
        Func<IRequestContext<TRequest>, CancellationToken, Task<IStatusResult<OperationStatus, TResponse>>> next
    )
    {
        var result = await next(request, ct);

        return Response.Result(request.Request.Rid, result);
    }
}