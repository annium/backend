using System.IO;
using System.Net.Http;
using System.Net.Mime;
using System.Text;

// ReSharper disable once CheckNamespace
namespace Annium.Net.Http;

public static class HttpRequestContentExtensions
{
    public static IHttpRequest JsonContent<T>(this IHttpRequest request, T data)
    {
        using var stream = request.Serializer.Serialize(MediaTypeNames.Application.Json, data);
        using var reader = new StreamReader(stream);
        var content = reader.ReadToEnd();

        return request.Attach(new StringContent(content, Encoding.UTF8, MediaTypeNames.Application.Json));
    }

    public static IHttpRequest StringContent(this IHttpRequest request, string data, string mimeType = "text/plain")
    {
        return request.Attach(new StringContent(data, Encoding.UTF8, mimeType));
    }
}