using System;
using System.IO;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using NodaTime;
using NodaTime.Serialization.JsonNet;

namespace Annium.Extensions.Net.Http
{
    public static class RunExtensions
    {
        private static JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings()
            .ConfigureAbstractConverter()
            .ConfigureForNodaTime(DateTimeZoneProviders.Serialization);

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

        public static async Task<T> AsAsync<T>(this IRequest request)
        {
            var response = await request.RunAsync();

            return parse(await response.Content.ReadAsStringAsync());

            T parse(string raw)
            {
                var mediaType = response.Content.Headers.ContentType.MediaType;

                switch (mediaType)
                {
                    case MediaTypeNames.Application.Json:
                        return JsonConvert.DeserializeObject<T>(raw, jsonSerializerSettings);
                    default:
                        throw new NotSupportedException($"Media type '{mediaType}' is not supported");
                }
            }
        }
    }
}