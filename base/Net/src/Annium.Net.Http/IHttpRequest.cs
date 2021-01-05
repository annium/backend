using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging.Abstractions;

namespace Annium.Net.Http
{
    public partial interface IHttpRequest
    {
        HttpMethod Method { get; }
        Uri Uri { get; }
        IReadOnlyDictionary<string, string> Params { get; }
        HttpContent? Content { get; }
        bool IsEnsuringSuccess { get; }
        IHttpContentSerializer ContentSerializer { get; }
        ILogger<IHttpRequest> Logger { get; }
        IHttpRequest Base(Uri baseUri);
        IHttpRequest Base(string baseUri);
        IHttpRequest UseClient(HttpClient client);
        IHttpRequest With(HttpMethod method, Uri uri);
        IHttpRequest With(HttpMethod method, string uri);
        IHttpRequest Param<T>(string key, T value);
        IHttpRequest Attach(HttpContent content);
        IHttpRequest EnsureSuccessStatusCode();
        IHttpRequest EnsureSuccessStatusCode(string message);
        IHttpRequest EnsureSuccessStatusCode(Func<IHttpResponse, string> getFailureMessage);
        IHttpRequest EnsureSuccessStatusCode(Func<IHttpResponse, Task<string>> getFailureMessage);
        IHttpRequest Clone();
        Task<IHttpResponse> RunAsync();
        Task<IHttpResponse> RunAsync(CancellationToken ct);
    }
}