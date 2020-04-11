using System;
using System.Threading.Tasks;

namespace Annium.Net.Http
{
    public partial interface IRequest
    {
        IRequest Configure(Action<IRequest> configure);
        IRequest Configure(Action<IRequest, RequestOptions> configure);
        IRequest Intercept(Func<Func<Task<IResponse>>, Task<IResponse>> middleware);
        IRequest Intercept(Func<Func<Task<IResponse>>, IRequest, Task<IResponse>> middleware);
        IRequest Intercept(Func<Func<Task<IResponse>>, IRequest, RequestOptions, Task<IResponse>> middleware);
    }
}