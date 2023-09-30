using System;

namespace Annium.Mesh.Domain.Responses;

public sealed class StreamChunkResponse<T> : ResponseBase
{
    public T Data { get; }

    public StreamChunkResponse(
        Guid rid,
        T data
    ) : base(rid)
    {
        Data = data;
    }
}