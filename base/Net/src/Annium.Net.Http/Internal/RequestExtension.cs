using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Annium.Net.Http.Internal
{
    internal partial class Request
    {
        public IRequest Configure(Action<IRequest> configure)
        {
            configure(this);

            return this;
        }

        public IRequest Configure(Action<IRequest, RequestOptions> configure)
        {
            var options = new RequestOptions(Method, GetUriFactory().Build(), _parameters, _headers, Content);
            configure(this, options);

            return this;
        }

        public IRequest Intercept(Func<Func<Task<IResponse>>, Task<IResponse>> middleware)
        {
            _middlewares.Add((next, request, options) => middleware(next));

            return this;
        }

        public IRequest Intercept(Func<Func<Task<IResponse>>, IRequest, Task<IResponse>> middleware)
        {
            _middlewares.Add((next, request, options) => middleware(next, request));

            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IRequest Intercept(Func<Func<Task<IResponse>>, IRequest, RequestOptions, Task<IResponse>> middleware)
        {
            _middlewares.Add(new Middleware(middleware));

            return this;
        }
    }
}