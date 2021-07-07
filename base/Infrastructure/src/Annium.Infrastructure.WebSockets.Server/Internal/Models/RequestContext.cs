using System;
using Annium.Infrastructure.WebSockets.Domain.Models;
using Annium.Infrastructure.WebSockets.Domain.Requests;
using Annium.Infrastructure.WebSockets.Server.Models;

namespace Annium.Infrastructure.WebSockets.Server.Internal.Models
{
    internal static class RequestContext
    {
        public static object CreateDynamic<TState>(AbstractRequestBase request, TState state)
            where TState : ConnectionStateBase
        {
            var type = typeof(RequestContext<,>).MakeGenericType(request.GetType(), typeof(TState));

            return Activator.CreateInstance(type, request, state)!;
        }
    }

    internal record RequestContext<TRequest, TState> : IRequestContext<TRequest, TState>
        where TState : ConnectionStateBase
    {
        public TRequest Request { get; }
        public TState State { get; }

        public RequestContext(
            TRequest request,
            TState state
        )
        {
            Request = request;
            State = state;
        }

        public void Deconstruct(
            out TRequest request,
            out TState state
        )
        {
            request = Request;
            state = State;
        }
    }
}