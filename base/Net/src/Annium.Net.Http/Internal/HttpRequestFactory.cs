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

        public IHttpRequest Get() => new HttpRequest(_httpContentSerializer);

        public IHttpRequest Get(string baseUri) => new HttpRequest(_httpContentSerializer, new Uri(baseUri.Trim()));

        public IHttpRequest Get(Uri baseUri) => new HttpRequest(_httpContentSerializer, baseUri);
    }
}