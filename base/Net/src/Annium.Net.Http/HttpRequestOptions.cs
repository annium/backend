using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http.Extensions;

namespace Annium.Net.Http
{
    public class HttpRequestOptions
    {
        public HttpMethod Method { get; }
        public Uri Uri { get; }
        public IReadOnlyDictionary<string, string> Parameters { get; }
        public string QueryString { get; }
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
            var qb = new QueryBuilder();
            foreach (var (key, value) in parameters)
                qb.Add(key, value);
            QueryString = qb.ToString();
            Headers = headers;
            Content = content;
        }
    }
}