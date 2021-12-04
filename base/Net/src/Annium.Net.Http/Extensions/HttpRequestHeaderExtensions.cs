using System;
using System.Net.Http.Headers;
using System.Text;

namespace Annium.Net.Http;

public static class HttpRequestHeaderExtensions
{
    public static IHttpRequest BasicAuthorization(this IHttpRequest request, string user, string pass) =>
        request.Authorization(new AuthenticationHeaderValue("Basic", $"{Convert.ToBase64String(Encoding.UTF8.GetBytes($"{user}:{pass}"))}"));

    public static IHttpRequest BearerAuthorization(this IHttpRequest request, string token) =>
        request.Authorization(new AuthenticationHeaderValue("Bearer", token));
}