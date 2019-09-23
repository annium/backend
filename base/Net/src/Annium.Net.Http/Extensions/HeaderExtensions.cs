using System;
using System.Net.Http.Headers;
using System.Text;

namespace Annium.Net.Http
{
    public static class HeaderExtensions
    {
        public static IRequest BasicAuthorization(this IRequest request, string user, string pass) =>
            request.Authorization(new AuthenticationHeaderValue("Basic", $"{Convert.ToBase64String(Encoding.UTF8.GetBytes($"{user}:{pass}"))}"));

        public static IRequest BearerAuthorization(this IRequest request, string token) =>
            request.Authorization(new AuthenticationHeaderValue("Bearer", token));
    }
}