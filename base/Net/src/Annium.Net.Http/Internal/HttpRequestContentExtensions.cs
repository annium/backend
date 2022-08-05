using System.Net.Http;
using System.Threading.Tasks;

namespace Annium.Net.Http.Internal;

internal static class HttpRequestContentExtensions
{
    public static async Task<T> ParseAsync<T>(
        this IHttpRequest request,
        HttpContent content
    )
    {
        var raw = await content.ReadAsStringAsync();

        var mediaType = content.Headers.ContentType?.MediaType
            ?? throw new HttpRequestException("Media-type missing in response");

        return request.ContentSerializer.Deserialize<T>(mediaType, raw);
    }

    public static async Task<T> ParseAsync<T>(
        this IHttpRequest request,
        HttpContent content,
        T defaultValue
    )
    {
        var raw = await content.ReadAsStringAsync();

        var mediaType = content.Headers.ContentType?.MediaType;
        if (mediaType is null || !request.ContentSerializer.CanSerialize(mediaType))
            return defaultValue;

        return request.ContentSerializer.Deserialize<T>(mediaType, raw);
    }
}