using System;
using Annium.Logging.Abstractions;

namespace Annium.Net.Http.Internal;

internal class HttpRequestFactory : IHttpRequestFactory
{
    private readonly IHttpContentSerializer _httpContentSerializer;
    private readonly ILogger<IHttpRequest> _logger;

    public HttpRequestFactory(
        IHttpContentSerializer httpContentSerializer,
        ILogger<IHttpRequest> logger
    )
    {
        _httpContentSerializer = httpContentSerializer;
        _logger = logger;
    }

    public IHttpRequest New() => new HttpRequest(_httpContentSerializer, _logger);

    public IHttpRequest New(string baseUri) => new HttpRequest(_httpContentSerializer, _logger, new Uri(baseUri.Trim()));

    public IHttpRequest New(Uri baseUri) => new HttpRequest(_httpContentSerializer, _logger, baseUri);
}