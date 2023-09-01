using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Net.Http.Internal;
using Microsoft.Extensions.Primitives;

namespace Annium.Net.Http;

public partial interface IHttpRequest : ILogSubject
{
    HttpMethod Method { get; }
    Uri Uri { get; }
    HttpRequestHeaders Headers { get; }
    IReadOnlyDictionary<string, StringValues> Params { get; }
    HttpContent? Content { get; }
    IHttpRequest Base(Uri baseUri);
    IHttpRequest Base(string baseUri);
    IHttpRequest UseClient(HttpClient client);
    IHttpRequest With(HttpMethod method, Uri uri);
    IHttpRequest With(HttpMethod method, string uri);
    IHttpRequest Param<T>(string key, List<T> values);
    IHttpRequest Param<T>(string key, IList<T> values);
    IHttpRequest Param<T>(string key, IReadOnlyList<T> values);
    IHttpRequest Param<T>(string key, IReadOnlyCollection<T> values);
    IHttpRequest Param<T>(string key, T[] values);
    IHttpRequest Param<T>(string key, IEnumerable<T> values);
    IHttpRequest Param<T>(string key, T value);
    IHttpRequest Attach(HttpContent content);
    IHttpRequest Clone();
    Task<IHttpResponse> RunAsync();
    Task<IHttpResponse> RunAsync(CancellationToken ct);
    internal IHttpContentSerializer ContentSerializer { get; }
}