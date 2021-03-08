using Annium.Infrastructure.WebSockets.Domain.Responses;

namespace Annium.Infrastructure.WebSockets.Server.Internal.Responses
{
    internal sealed class MetaResponse<T, TR> : AbstractResponseBase, IMetaResponse
        where TR : AbstractResponseBase
    {
        public AbstractResponseBase Response { get; }

        public MetaResponse(TR response)
        {
            Response = response;
        }
    }

    internal sealed class MetaResponse<T1, T2, TR> : AbstractResponseBase, IMetaResponse
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