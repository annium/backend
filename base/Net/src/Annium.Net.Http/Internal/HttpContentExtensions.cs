using System;
using System.Net.Http;
using System.Net.Mime;
using System.Text.Json;
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
                MediaTypeNames.Application.Json => JsonSerializer.Deserialize<T>(raw, Options.Json),
                _ => throw new NotSupportedException($"Media type '{mediaType}' is not supported"),
            };
        }
    }
}