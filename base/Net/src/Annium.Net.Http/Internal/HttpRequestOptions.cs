using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using Annium.Net.Base;
using Microsoft.Extensions.Primitives;

namespace Annium.Net.Http.Internal;

internal class HttpRequestOptions : IHttpRequestOptions
{
    public HttpMethod Method { get; }
    public Uri Uri { get; }
    public IReadOnlyDictionary<string, StringValues> Params { get; }
    public string QueryString { get; }
    public HttpRequestHeaders Headers { get; }
    public HttpContent? Content { get; }

    public HttpRequestOptions(
        HttpMethod method,
        Uri uri,
        IReadOnlyDictionary<string, StringValues> parameters,
        HttpRequestHeaders headers,
        HttpContent? content
    )
    {
        Method = method;
        Uri = uri;
        Params = parameters;

        var query = UriQuery.New();
        foreach (var (key, values) in parameters)
            query[key] = values;
        QueryString = query.ToString();
        Headers = headers;
        Content = content;
    }
}