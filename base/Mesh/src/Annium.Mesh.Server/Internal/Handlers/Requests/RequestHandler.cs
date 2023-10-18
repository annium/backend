// using System;
// using System.Threading;
// using System.Threading.Tasks;
// using Annium.Architecture.Base;
// using Annium.Data.Operations;
// using Annium.Mesh.Domain;
// using Annium.Mesh.Server.Models;
// using Response = Annium.Mesh.Server.Internal.Responses.Response;
//
// namespace Annium.Mesh.Server.Internal.Handlers.Requests;
//
// internal class RequestHandler<TRequest>
//     where TRequest : RequestBase
// {
//     public async Task<Domain.Response> HandleAsync(
//         TRequest request,
//         CancellationToken ct,
//         Func<IRequestContext<TRequest>, CancellationToken, Task<IStatusResult<OperationStatus>>> next
//     )
//     {
//         var result = await next(request, ct);
//
//         return Response.Result(request.Request.Rid, result);
//     }
// }