using System;
using System.Threading.Tasks;
using Annium.Core.Mediator;
using Annium.Data.Operations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Annium.AspNetCore.Extensions
{
    public class ServerController : ControllerBase
    {
        protected readonly IMediator mediator;

        protected ServerController(
            IMediator mediator
        )
        {
            this.mediator = mediator;
        }

        [NonAction]
        protected Task<IResult<TResponse>> HandleAsync<TRequest, TResponse>(TRequest request)
        {
            return mediator.SendAsync<ValueTuple<ModelStateDictionary, TRequest>, IResult<TResponse>>((ModelState, request));
        }

        [NonAction]
        protected Task HandleAsync<TRequest>(TRequest request)
        {
            return mediator.SendAsync<ValueTuple<ModelStateDictionary, TRequest>, IResult>((ModelState, request));
        }
    }
}