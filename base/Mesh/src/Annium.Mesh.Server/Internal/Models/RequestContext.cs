using System;
using Annium.Mesh.Server.Models;

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

internal record RequestContext<TRequest>(Guid ConnectionId, TRequest Request) : IRequestContext<TRequest>;
