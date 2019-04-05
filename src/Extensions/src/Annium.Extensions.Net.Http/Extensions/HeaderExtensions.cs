using System.Net.Http.Headers;

namespace Annium.Extensions.Net.Http
{
    public static class HeaderExtensions
    {
        public static IRequest BearerAuthorization(this IRequest request, string token) =>
            request.Authorization(new AuthenticationHeaderValue("Bearer", token));
    }
}