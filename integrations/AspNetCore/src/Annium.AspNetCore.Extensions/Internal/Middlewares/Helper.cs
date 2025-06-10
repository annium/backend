using System.Net;
using System.Threading.Tasks;
using Annium.Data.Operations;
using Annium.Serialization.Abstractions;
using Microsoft.AspNetCore.Http;

namespace Annium.AspNetCore.Extensions.Internal.Middlewares;

/// <summary>
/// Helper class for writing HTTP responses with serialized result objects
/// </summary>
internal class Helper
{
    /// <summary>
    /// The serializer for converting results to strings
    /// </summary>
    private readonly ISerializer<string> _serializer;

    /// <summary>
    /// The media type for the response content
    /// </summary>
    private readonly string _mediaType;

    /// <summary>
    /// Initializes a new instance of the Helper class
    /// </summary>
    /// <param name="serializer">The serializer for converting results to strings</param>
    /// <param name="mediaType">The media type for the response content</param>
    public Helper(ISerializer<string> serializer, string mediaType)
    {
        _serializer = serializer;
        _mediaType = mediaType;
    }

    /// <summary>
    /// Writes a serialized result to the HTTP response
    /// </summary>
    /// <param name="context">The HTTP context</param>
    /// <param name="status">The HTTP status code</param>
    /// <param name="result">The result to serialize and write</param>
    /// <returns>A task that represents the asynchronous write operation</returns>
    public Task WriteResponseAsync(HttpContext context, HttpStatusCode status, IResultBase result)
    {
        context.Response.StatusCode = (int)status;
        context.Response.ContentType = _mediaType;

        return context.Response.WriteAsync(_serializer.Serialize(result));
    }
}
