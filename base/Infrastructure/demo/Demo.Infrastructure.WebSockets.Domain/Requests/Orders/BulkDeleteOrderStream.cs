using System;
using Annium.Infrastructure.WebSockets.Domain.Requests;

namespace Demo.Infrastructure.WebSockets.Domain.Requests.Orders
{
    // request stream -> void
    public record BulkDeleteOrderStream : StreamChunkRequestBase
    {
        public Guid Id { get; init; }
    }
}