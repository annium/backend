using System;
using Annium.Infrastructure.WebSockets.Domain.Requests;

namespace Demo.Infrastructure.WebSockets.Domain.Requests.Orders
{
    // request stream -> response
    public record UploadOrderFileRequest : StreamHeadRequestBase
    {
        public string Name { get; init; } = string.Empty;
    }

    public record UploadOrderFileStreamChunkRequest : StreamChunkRequestBase
    {
        public ReadOnlyMemory<byte> Chunk { get; init; }
    }
}