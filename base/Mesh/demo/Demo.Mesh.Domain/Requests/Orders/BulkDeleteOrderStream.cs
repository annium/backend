using System;
using Annium.Mesh.Domain.Requests;

namespace Demo.Mesh.Domain.Requests.Orders;

// request stream -> void
public class BulkDeleteOrderStream : StreamChunkRequestBase
{
    public Guid Id { get; init; }
}