using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using Annium.Net.Base;
using Microsoft.Extensions.Primitives;

namespace Annium.Net.Http;

public class HttpRequestOptions
{
    public HttpMethod Method { get; }
    public Uri Uri { get; }
    public IReadOnlyDictionary<string, StringValues> Parameters { get; }
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
        Parameters = parameters;

        var query = UriQuery.New();
        foreach (var (key, values) in parameters)
            query[key] = values;
        QueryString = query.ToString();
        Headers = headers;
        Content = content;
    }
}