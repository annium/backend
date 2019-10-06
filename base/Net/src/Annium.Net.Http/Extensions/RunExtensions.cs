using System;
using System.IO;
using System.Net.Mime;
using System.Text.Json;
using System.Threading.Tasks;
using Annium.Data.Operations;
using Annium.Net.Http.Internal;

namespace Annium.Net.Http
{
    public static class RunExtensions
    {
        public static async Task<string> AsStringAsync(this IRequest request)
        {
            var response = await request.RunAsync();

            return await response.Content.ReadAsStringAsync();
        }

        public static async Task<byte[]> AsByteArrayAsync(this IRequest request)
        {
            var response = await request.RunAsync();

            return await response.Content.ReadAsByteArrayAsync();
        }

        public static async Task<Stream> AsStreamAsync(this IRequest request)
        {
            var response = await request.RunAsync();

            return await response.Content.ReadAsStreamAsync();
        }

        public static Task<IResult<T>> AsResultAsync<T>(this IRequest request) => request.AsAsync<IResult<T>>();

        public static async Task<T> AsAsync<T>(this IRequest request)
        {
            var response = await request.RunAsync();
            if (!response.IsSuccessStatusCode)
                return default !;

            return parse(await response.Content.ReadAsStringAsync());

            T parse(string raw)
            {
                var mediaType = response.Content.Headers.ContentType.MediaType;

                return mediaType
                switch
                {
                    MediaTypeNames.Application.Json => JsonSerializer.Deserialize<T>(raw, Options.Json),
                        _ =>
                        throw new NotSupportedException($"Media type '{mediaType}' is not supported"),
                };
            }
        }
    }
}