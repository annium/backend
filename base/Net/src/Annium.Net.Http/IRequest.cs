using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Annium.Net.Http
{
    public partial interface IRequest
    {
        IRequest Base(Uri baseUri);

        IRequest Base(string baseUri);

        IRequest UseClient(HttpClient client);

        IRequest UseClientFactory(Func<HttpClient> createClient);

        IRequest Method(HttpMethod method, Uri uri);

        IRequest Method(HttpMethod method, string uri);

        IRequest Param<T>(string key, T value);

        IRequest Content(HttpContent content);

        IRequest EnsureSuccessStatusCode();

        IRequest EnsureSuccessStatusCode(string message);

        IRequest EnsureSuccessStatusCode(Func<IResponse, string> getFailureMessage);

        IRequest EnsureSuccessStatusCode(Func<IResponse, Task<string>> getFailureMessage);

        IRequest Configure(
            Action<IRequest, HttpMethod, Uri, IReadOnlyDictionary<string, string>, HttpRequestHeaders, HttpContent?> configure
        );

        IRequest Clone();

        Task<IResponse> RunAsync();
    }
}