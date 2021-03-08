using Annium.Infrastructure.WebSockets.Domain.Responses;

namespace Annium.Infrastructure.WebSockets.Server.Internal.Responses
{
    internal sealed class VoidResponse : AbstractResponseBase, IVoidResponse
    {
    }

    internal sealed class VoidResponse<T> : AbstractResponseBase, IVoidResponse
    {
    }

    internal sealed class VoidResponse<T1, T2> : AbstractResponseBase, IVoidResponse
    {
    }

    interface IVoidResponse
    {
    }
}