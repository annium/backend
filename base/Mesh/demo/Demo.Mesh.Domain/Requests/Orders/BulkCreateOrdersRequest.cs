using Annium.Mesh.Domain.Requests;

namespace Demo.Mesh.Domain.Requests.Orders;

// request stream -> response stream
public class BulkCreateOrdersRequest : StreamHeadRequestBase
{
    public string Customer { get; init; } = string.Empty;
}

public class BulkCreateOrderStreamChunkRequest : StreamChunkRequestBase
{
    public string Product { get; init; } = string.Empty;
}