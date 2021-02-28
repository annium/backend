using System;

namespace Annium.Infrastructure.WebSockets.Domain.Responses
{
    public sealed record StreamChunkResponse<T> : ResponseBase
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
}