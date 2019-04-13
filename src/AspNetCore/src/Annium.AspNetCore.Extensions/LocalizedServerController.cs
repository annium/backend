using System.Linq;
using System.Net;
using Annium.Data.Operations;
using Annium.Extensions.Primitives;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Localization;

namespace Annium.AspNetCore.Extensions
{
    public class LocalizedServerController : ServerController
    {
        protected readonly IStringLocalizer localizer;

        protected LocalizedServerController(
            IStringLocalizer localizer
        )
        {
            this.localizer = localizer;
        }

        [NonAction]
        public override IActionResult Conflict(string error) =>
            new ObjectResult(Result.Failure().Error(localizer[error])) { StatusCode = (int) HttpStatusCode.Conflict };

        [NonAction]
        public override BadRequestObjectResult BadRequest(ModelStateDictionary modelState)
        {
            var result = Result.Failure();

            foreach (var(field, entry) in modelState)
                result.Error(field.CamelCase(), string.Join("; ", entry.Errors.Select(e => localizer[e.ErrorMessage])));

            return new BadRequestObjectResult(result);
        }

        [NonAction]
        public override IActionResult BadRequest(string error) =>
            new BadRequestObjectResult(Result.Failure().Error(localizer[error]));

        [NonAction]
        public override IActionResult Forbidden(string error) =>
            new ObjectResult(Result.Failure().Error(localizer[error])) { StatusCode = (int) HttpStatusCode.Forbidden };

        [NonAction]
        public override IActionResult ServerError(string error) =>
            new ObjectResult(Result.Failure().Error(localizer[error])) { StatusCode = (int) HttpStatusCode.InternalServerError };
    }
}