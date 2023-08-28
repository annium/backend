using System;
using Annium.Logging;

namespace Annium.Net.Http.Internal;

internal class HttpRequestFactory : IHttpRequestFactory
{
    private readonly IHttpContentSerializer _httpContentSerializer;
    private readonly ILogger _logger;

    public HttpRequestFactory(
        IHttpContentSerializer httpContentSerializer,
        ILogger logger
    )
    {
        _httpContentSerializer = httpContentSerializer;
        _logger = logger;
    }

    public IHttpRequest New() => new HttpRequest(_httpContentSerializer, _logger);

    public IHttpRequest New(string baseUri) => new HttpRequest(_httpContentSerializer, _logger, new Uri(baseUri.Trim()));

    public IHttpRequest New(Uri baseUri) => new HttpRequest(_httpContentSerializer, _logger, baseUri);
}