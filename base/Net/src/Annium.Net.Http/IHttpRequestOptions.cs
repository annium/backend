using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.Primitives;

namespace Annium.Net.Http;

public interface IHttpRequestOptions
{
    public HttpMethod Method { get; }
    public Uri Uri { get; }
    public IReadOnlyDictionary<string, StringValues> Parameters { get; }
    public string QueryString { get; }
    public HttpRequestHeaders Headers { get; }
    public HttpContent? Content { get; }
}