using System;
using Annium.Infrastructure.WebSockets.Domain.Requests;

namespace Demo.Infrastructure.WebSockets.Domain.Requests.Orders;

// request stream -> void
public class BulkDeleteOrderStream : StreamChunkRequestBase
{
    public Guid Id { get; init; }
}