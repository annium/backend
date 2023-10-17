using System;
using Annium.Mesh.Domain.Requests;
using Annium.Mesh.Server.Models;

namespace Annium.Mesh.Server.Internal.Models;

internal static class RequestContext
{
    public static object CreateDynamic(AbstractRequestBase request, ConnectionState state)
    {
        var type = typeof(RequestContext<>).MakeGenericType(request.GetType());

        return Activator.CreateInstance(type, request, state)!;
    }
}

internal record RequestContext<TRequest> : IRequestContext<TRequest>
{
    public TRequest Request { get; }
    public ConnectionState State { get; }

    public RequestContext(
        TRequest request,
        ConnectionState state
    )
    {
        Request = request;
        State = state;
    }

    public void Deconstruct(
        out TRequest request,
        out ConnectionState state
    )
    {
        request = Request;
        state = State;
    }
}