using System;

namespace Annium.Mesh.Server.Internal.Models;

// internal static class RequestContext
// {
//     public static object CreateDynamic(Guid cid, AbstractRequestBaseObsolete request)
//     {
//         var type = typeof(RequestContext<>).MakeGenericType(request.GetType());
//
//         return Activator.CreateInstance(type, cid, request)!;
//     }
// }

/// <summary>
/// Represents the context for a request, containing the connection ID and the request data.
/// </summary>
/// <typeparam name="TRequest">The type of the request.</typeparam>
/// <param name="ConnectionId">The identifier of the connection that made the request.</param>
/// <param name="Request">The request data.</param>
internal record RequestContext<TRequest>(Guid ConnectionId, TRequest Request) : IRequestContext<TRequest>;
