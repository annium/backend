using System;
using Annium.Infrastructure.WebSockets.Domain.Models;
using Annium.Infrastructure.WebSockets.Domain.Requests;

namespace Annium.Infrastructure.WebSockets.Server.Internal.Models
{
    internal static class RequestContext
    {
        public static object CreateDynamic(AbstractRequestBase request, IConnectionState state)
        {
            var type = typeof(RequestContext<>).MakeGenericType(request.GetType());

            return Activator.CreateInstance(type, request, state)!;
        }
    }

    internal record RequestContext<TRequest> : IRequestContext<TRequest>, IRequestContextInternal
    {
        public TRequest Request { get; }
        public IConnectionState State => StateInternal;
        public ConnectionState StateInternal { get; }

        public RequestContext(
            TRequest request,
            ConnectionState state1
        )
        {
            Request = request;
            StateInternal = state1;
        }

        public void Deconstruct(
            out TRequest request,
            out IConnectionState state
        )
        {
            request = Request;
            state = State;
        }
    }
}