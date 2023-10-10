using System.Net;
using System.Threading.Tasks;
using Annium.Data.Operations;
using Annium.Serialization.Abstractions;
using Microsoft.AspNetCore.Http;

namespace Annium.AspNetCore.Mesh.Internal.Middleware;

internal class Helper
{
    private readonly ISerializer<string> _serializer;
    private readonly string _mediaType;

    public Helper(
        ISerializer<string> serializer,
        string mediaType
    )
    {
        _serializer = serializer;
        _mediaType = mediaType;
    }

    public Task WriteResponse(HttpContext context, HttpStatusCode status, IResultBase result)
    {
        context.Response.StatusCode = (int)status;
        context.Response.ContentType = _mediaType;

        return context.Response.WriteAsync(_serializer.Serialize(result));
    }
}