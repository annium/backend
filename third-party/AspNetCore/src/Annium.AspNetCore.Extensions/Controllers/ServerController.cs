using System.Threading.Tasks;
using Annium.Core.Mediator;
using Annium.Data.Operations;
using Microsoft.AspNetCore.Mvc;

namespace Annium.AspNetCore.Extensions
{
    public class ServerController : ControllerBase
    {
        private readonly IMediator _mediator;

        protected ServerController(
            IMediator mediator
        )
        {
            _mediator = mediator;
        }

        [NonAction]
        protected Task<IResult<TResponse>> HandleAsync<TRequest, TResponse>(TRequest request)
        {
            return _mediator.SendAsync<TRequest, IResult<TResponse>>(request);
        }

        [NonAction]
        protected Task<IResult> HandleAsync<TRequest>(TRequest request)
        {
            return _mediator.SendAsync<TRequest, IResult>(request);
        }
    }
}