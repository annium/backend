using System.Net.Http;
using System.Net.Mime;
using System.Text;
using Newtonsoft.Json;

namespace Annium.Extensions.Net.Http
{
    public static class ContentExtensions
    {
        public static IRequest JsonContent<T>(this IRequest request, T data) =>
            request.Content(new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, MediaTypeNames.Application.Json));
    }
}