using System.Net.Http;
using System.Net.Mime;
using System.Text;
using Annium.Net.Http.Internal;

namespace Annium.Net.Http
{
    public static class ContentExtensions
    {
        public static IRequest JsonContent<T>(this IRequest request, T data)
        {
            var content = Serializers.Json.Serialize(data);

            return request.Attach(new StringContent(content, Encoding.UTF8, MediaTypeNames.Application.Json));
        }
    }
}