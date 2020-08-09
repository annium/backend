using System;

namespace Annium.Net.Http.Internal
{
    internal class HttpRequestFactory : IHttpRequestFactory
    {
        private readonly IHttpContentSerializer _httpContentSerializer;

        public HttpRequestFactory(
            IHttpContentSerializer httpContentSerializer
        )
        {
            _httpContentSerializer = httpContentSerializer;
        }

        public IHttpRequest New() => new HttpRequest(_httpContentSerializer);

        public IHttpRequest New(string baseUri) => new HttpRequest(_httpContentSerializer, new Uri(baseUri.Trim()));

        public IHttpRequest New(Uri baseUri) => new HttpRequest(_httpContentSerializer, baseUri);
    }
}