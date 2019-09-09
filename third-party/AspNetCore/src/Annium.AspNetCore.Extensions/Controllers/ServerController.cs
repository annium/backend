using System;
using System.Net;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Core.Mediator;
using Annium.Data.Operations;
using Annium.Extensions.Primitives;
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
        public IActionResult Created(object result) =>
            new ObjectResult(result) { StatusCode = (int) HttpStatusCode.Created };

        [NonAction]
        public override BadRequestObjectResult BadRequest(ModelStateDictionary modelState)
        {
            var result = Result.New();

            foreach (var(field, entry) in modelState)
            {
                var label = field.CamelCase();
                foreach (var error in entry.Errors)
                    result.Error(label, error.ErrorMessage);
            }

            return new BadRequestObjectResult(result);
        }

        [NonAction]
        public virtual IActionResult BadRequest(string error) =>
            new BadRequestObjectResult(Result.New().Error(error));

        [NonAction]
        public IActionResult Forbidden(IResultBase result) =>
            new ObjectResult(result) { StatusCode = (int) HttpStatusCode.Forbidden };

        [NonAction]
        public virtual IActionResult Forbidden(string error) =>
            new ObjectResult(Result.New().Error(error)) { StatusCode = (int) HttpStatusCode.Forbidden };

        [NonAction]
        public IActionResult Conflict(IResultBase result) =>
            new ObjectResult(result) { StatusCode = (int) HttpStatusCode.Conflict };

        [NonAction]
        public virtual IActionResult Conflict(string error) =>
            new ObjectResult(Result.New().Error(error)) { StatusCode = (int) HttpStatusCode.Conflict };

        [NonAction]
        public IActionResult NotFound(IResultBase result) =>
            new ObjectResult(result) { StatusCode = (int) HttpStatusCode.NotFound };

        [NonAction]
        public virtual IActionResult NotFound(string error) =>
            new ObjectResult(Result.New().Error(error)) { StatusCode = (int) HttpStatusCode.NotFound };

        [NonAction]
        public IActionResult ServerError(IResultBase result) =>
            new ObjectResult(result) { StatusCode = (int) HttpStatusCode.InternalServerError };

        [NonAction]
        public virtual IActionResult ServerError(string error) =>
            new ObjectResult(Result.New().Error(error)) { StatusCode = (int) HttpStatusCode.InternalServerError };

        [NonAction]
        protected async Task<IActionResult> HandleAsync<TRequest, TResponse>(TRequest request)
        {
            var result = await mediator.SendAsync<ValueTuple<ModelStateDictionary, TRequest>, IStatusResult<HttpStatusCode, TResponse>>((ModelState, request));

            return new ObjectResult(Result.New().Join(result)) { StatusCode = (int) result.Status };
        }
    }
}