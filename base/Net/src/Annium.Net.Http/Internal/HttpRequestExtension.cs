using System;
using System.Threading.Tasks;

namespace Annium.Net.Http.Internal;

internal partial class HttpRequest
{
    public IHttpRequest Configure(
        Action<IHttpRequest> configure
    )
    {
        configure(this);

        return this;
    }

    public IHttpRequest Configure(
        Action<IHttpRequest, IHttpRequestOptions> configure
    )
    {
        var options = new HttpRequestOptions(Method, Path, _parameters, Headers, Content);
        configure(this, options);

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