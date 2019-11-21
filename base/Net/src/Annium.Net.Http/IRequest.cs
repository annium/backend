using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Annium.Net.Http
{
    public partial interface IRequest
    {
        bool IsEnsuringSuccess { get; }
        IRequest Base(Uri baseUri);
        IRequest Base(string baseUri);
        IRequest UseClient(HttpClient client);
        IRequest UseClientFactory(Func<HttpClient> createClient);
        IRequest With(HttpMethod method, Uri uri);
        IRequest With(HttpMethod method, string uri);
        IRequest Param<T>(string key, T value);
        IRequest Content(HttpContent content);
        IRequest Configure(
            Action<IRequest, HttpMethod, Uri, IReadOnlyDictionary<string, string>, HttpRequestHeaders, HttpContent?> configure
        );
        IRequest EnsureSuccessStatusCode();
        IRequest EnsureSuccessStatusCode(string message);
        IRequest EnsureSuccessStatusCode(Func<IResponse, string> getFailureMessage);
        IRequest EnsureSuccessStatusCode(Func<IResponse, Task<string>> getFailureMessage);
        IRequest Clone();
        Task<IResponse> RunAsync();
    }
}