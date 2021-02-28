using Annium.Infrastructure.WebSockets.Domain.Requests;

namespace Demo.Infrastructure.WebSockets.Domain.Requests.Orders
{
    // request stream -> response stream
    public record BulkCreateOrdersRequest : StreamHeadRequestBase
    {
        public string Customer { get; init; } = string.Empty;
    }

    public record BulkCreateOrderStreamChunkRequest : StreamChunkRequestBase
    {
        public string Product { get; init; } = string.Empty;
    }
}