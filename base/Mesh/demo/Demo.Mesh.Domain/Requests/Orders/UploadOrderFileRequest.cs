using System;
using Annium.Mesh.Domain.Requests;

namespace Demo.Mesh.Domain.Requests.Orders;

// request stream -> response
public class UploadOrderFileRequest : StreamHeadRequestBase
{
    public string Name { get; init; } = string.Empty;
}

public class UploadOrderFileStreamChunkRequest : StreamChunkRequestBase
{
    public ReadOnlyMemory<byte> Chunk { get; init; }
}