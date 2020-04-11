using System;
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
            throw new NotImplementedException();
        }

        public IRequest Intercept(Func<Func<Task<IResponse>>, IRequest, Task<IResponse>> middleware)
        {
            throw new NotImplementedException();
        }

        public IRequest Intercept(Func<Func<Task<IResponse>>, IRequest, RequestOptions, Task<IResponse>> middleware)
        {
            throw new NotImplementedException();
        }
    }
}