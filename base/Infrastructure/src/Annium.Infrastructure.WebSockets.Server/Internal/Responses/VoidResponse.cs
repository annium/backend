using Annium.Infrastructure.WebSockets.Domain.Responses;

namespace Annium.Infrastructure.WebSockets.Server.Internal.Responses
{
    internal sealed record VoidResponse : AbstractResponseBase, IVoidResponse;

    internal sealed record VoidResponse<T> : AbstractResponseBase, IVoidResponse;

    internal sealed record VoidResponse<T1, T2> : AbstractResponseBase, IVoidResponse;

    interface IVoidResponse
    {
    }
}