using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Annium.Extensions.Net.Http
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

        IRequest Clone();

        Task<IResponse> RunAsync();
    }
}