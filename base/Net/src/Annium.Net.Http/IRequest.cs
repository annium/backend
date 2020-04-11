using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Annium.Net.Http
{
    public partial interface IRequest
    {
        HttpMethod Method { get; }
        Uri Uri { get; }
        IReadOnlyDictionary<string, string> Params { get; }
        HttpContent? Content { get; }
        bool IsEnsuringSuccess { get; }
        IRequest Base(Uri baseUri);
        IRequest Base(string baseUri);
        IRequest UseClient(HttpClient client);
        IRequest With(HttpMethod method, Uri uri);
        IRequest With(HttpMethod method, string uri);
        IRequest Param<T>(string key, T value);
        IRequest Attach(HttpContent content);
        IRequest EnsureSuccessStatusCode();
        IRequest EnsureSuccessStatusCode(string message);
        IRequest EnsureSuccessStatusCode(Func<IResponse, string> getFailureMessage);
        IRequest EnsureSuccessStatusCode(Func<IResponse, Task<string>> getFailureMessage);
        IRequest Clone();
        Task<IResponse> RunAsync();
    }
}