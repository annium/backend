using Annium.Mesh.Domain.Responses;

namespace Annium.Mesh.Server.Internal.Responses;

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