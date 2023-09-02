using System;
using System.Threading.Tasks;

namespace Annium.Net.Http.Internal;

internal partial class HttpRequest
{
    public IHttpRequest Configure(
        Action<IHttpRequest> configure
    )
    {
        _configurations.Add((request, _) => configure(request));

        return this;
    }

    public IHttpRequest Configure(
        Action<IHttpRequest, IHttpRequestOptions> configure
    )
    {
        _configurations.Add(new Configuration(configure));

        return this;
    }

    public IHttpRequest Intercept(
        Func<Func<Task<IHttpResponse>>, Task<IHttpResponse>> middleware
    )
    {
        _middlewares.Add((next, _, _) => middleware(next));

        return this;
    }

    public IHttpRequest Intercept(
        Func<Func<Task<IHttpResponse>>, IHttpRequest, Task<IHttpResponse>> middleware
    )
    {
        _middlewares.Add((next, request, _) => middleware(next, request));

        return this;
    }

    public IHttpRequest Intercept(
        Func<Func<Task<IHttpResponse>>, IHttpRequest, IHttpRequestOptions, Task<IHttpResponse>> middleware
    )
    {
        _middlewares.Add(new Middleware(middleware));

        return this;
    }
}