using Annium.Infrastructure.WebSockets.Domain.Responses;

namespace Annium.Infrastructure.WebSockets.Server.Internal.Responses
{
    internal sealed record MetaResponse<T, TR> : AbstractResponseBase, IMetaResponse
        where TR : AbstractResponseBase
    {
        public AbstractResponseBase Response { get; }

        public MetaResponse(TR response)
        {
            Response = response;
        }
    }

    internal sealed record MetaResponse<T1, T2, TR> : AbstractResponseBase, IMetaResponse
        where TR : AbstractResponseBase
    {
        public AbstractResponseBase Response { get; }

        public MetaResponse(TR response)
        {
            Response = response;
        }
    }

    interface IMetaResponse
    {
        AbstractResponseBase Response { get; }
    }
}