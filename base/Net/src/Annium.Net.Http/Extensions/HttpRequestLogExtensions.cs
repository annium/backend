using System;
using System.Net;
using System.Net.Http;
using Annium.Logging;

// ReSharper disable once CheckNamespace
namespace Annium.Net.Http;

public static class HttpRequestLogExtensions
{
    public static IHttpRequest WithLogFrom<T>(this IHttpRequest request, T subject)
        where T : ILogSubject => request.Intercept(async next =>
    {
        var id = Guid.NewGuid();
        var response = default(IHttpResponse);
        var responseString = string.Empty;
        try
        {
            subject.Trace(
                "request {id}: {method} {uri}",
                id,
                request.Method,
                request.Uri
            );

            response = await next();
            responseString = await response.Content.ReadAsStringAsync();
            return response;
        }
        catch (Exception e)
        {
            subject.Trace(
                "failed {id}: {method} {uri}: {e}",
                id,
                request.Method,
                request.Uri,
                e
            );
            throw;
        }
        finally
        {
            if (response is not null)
            {
                subject.Trace<Guid, HttpMethod, Uri, HttpStatusCode, string, string>(
                    "response {id}: {method} {uri} -> {statusCode} ({statusText})\n{response}",
                    id,
                    request.Method,
                    request.Uri,
                    response.StatusCode,
                    response.StatusText,
                    responseString
                );
            }
        }
    });
}