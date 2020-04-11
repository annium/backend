using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Annium.Net.Http
{
    public class HttpRequestOptions
    {
        public HttpMethod Method { get; }
        public Uri Uri { get; }
        public IReadOnlyDictionary<string, string> Parameters { get; }
        public HttpRequestHeaders Headers { get; }
        public HttpContent? Content { get; }

        public HttpRequestOptions(
            HttpMethod method,
            Uri uri,
            IReadOnlyDictionary<string, string> parameters,
            HttpRequestHeaders headers,
            HttpContent? content
        )
        {
            Method = method;
            Uri = uri;
            Parameters = parameters;
            Headers = headers;
            Content = content;
        }
    }
}