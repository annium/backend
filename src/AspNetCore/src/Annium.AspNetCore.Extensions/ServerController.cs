using System.Linq;
using System.Net;
using Annium.Data.Operations;
using Annium.Extensions.Primitives;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Annium.AspNetCore.Extensions
{
    public class ServerController : ControllerBase
    {

        [NonAction]
        public IActionResult Created(object result) =>
            new ObjectResult(result) { StatusCode = (int) HttpStatusCode.Created };

        [NonAction]
        public IActionResult Conflict(IResult result) =>
            new ObjectResult(result) { StatusCode = (int) HttpStatusCode.Conflict };

        [NonAction]
        public override BadRequestObjectResult BadRequest(ModelStateDictionary modelState)
        {
            var result = Result.Failure();

            foreach (var(field, entry) in modelState)
                result.Error(field.CamelCase(), string.Join("; ", entry.Errors.Select(e => e.ErrorMessage)));

            return new BadRequestObjectResult(result);
        }

        [NonAction]
        public IActionResult Forbidden(IResult result) =>
            new ObjectResult(result) { StatusCode = (int) HttpStatusCode.Forbidden };

        [NonAction]
        public IActionResult ServerError(IResult result) =>
            new ObjectResult(result) { StatusCode = (int) HttpStatusCode.InternalServerError };
    }
}