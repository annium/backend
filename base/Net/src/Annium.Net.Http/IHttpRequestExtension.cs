using System;
using System.Threading.Tasks;

namespace Annium.Net.Http;

public partial interface IHttpRequest
{
    IHttpRequest Configure(Action<IHttpRequest> configure);
    IHttpRequest Configure(Action<IHttpRequest, IHttpRequestOptions> configure);
    IHttpRequest Intercept(Func<Func<Task<IHttpResponse>>, Task<IHttpResponse>> middleware);
    IHttpRequest Intercept(Func<Func<Task<IHttpResponse>>, IHttpRequest, Task<IHttpResponse>> middleware);
    IHttpRequest Intercept(Func<Func<Task<IHttpResponse>>, IHttpRequest, IHttpRequestOptions, Task<IHttpResponse>> middleware);
}