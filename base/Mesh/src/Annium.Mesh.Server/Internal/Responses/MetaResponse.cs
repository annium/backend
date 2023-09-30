using Annium.Mesh.Domain.Responses;

namespace Annium.Mesh.Server.Internal.Responses;

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