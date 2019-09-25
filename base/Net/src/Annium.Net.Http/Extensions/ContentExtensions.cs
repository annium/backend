using System.Net.Http;
using System.Net.Mime;
using System.Text;
using Newtonsoft.Json;

namespace Annium.Net.Http
{
    public static class ContentExtensions
    {
        public static IRequest JsonContent<T>(this IRequest request, T data)
        {
            var settings = new JsonSerializerSettings();
            settings.NullValueHandling = NullValueHandling.Ignore;
            var content = JsonConvert.SerializeObject(data, settings);

            return request.Content(new StringContent(content, Encoding.UTF8, MediaTypeNames.Application.Json));
        }
    }
}