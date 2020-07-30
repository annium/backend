using System.Net.Http;
using System.Net.Mime;
using System.Text;

namespace Annium.Net.Http
{
    public static class HttpRequestContentExtensions
    {
        public static IHttpRequest JsonContent<T>(this IHttpRequest request, T data)
        {
            var content = request.ContentSerializer.Serialize(MediaTypeNames.Application.Json, data);

            return request.Attach(new StringContent(content, Encoding.UTF8, MediaTypeNames.Application.Json));
        }
    }
}