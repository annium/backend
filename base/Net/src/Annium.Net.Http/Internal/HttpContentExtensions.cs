using System;
using System.Net.Http;
using System.Net.Mime;
using System.Threading.Tasks;

namespace Annium.Net.Http.Internal
{
    internal static class HttpContentExtensions
    {
        public static async Task<T> ParseAsync<T>(this HttpContent content)
        {
            var raw = await content.ReadAsStringAsync();

            var mediaType = content.Headers.ContentType.MediaType;

            return mediaType switch
            {
                MediaTypeNames.Application.Json => Serializers.Json.Deserialize<T>(raw),
                _ => throw new NotSupportedException($"Media type '{mediaType}' is not supported"),
            };
        }
    }
}