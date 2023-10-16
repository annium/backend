using Annium.Mesh.Domain.Responses;

namespace Annium.Mesh.Server.Internal.Responses;

internal sealed class VoidResponse<T> : AbstractResponseBase, IVoidResponse
{
}

interface IVoidResponse
{
}