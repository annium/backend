using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Mediator;
using Annium.Data.Operations;
using Annium.Extensions.Validation;
using Annium.Logging.Abstractions;

namespace Annium.Architecture.Mediator.PipeHandlers
{
    internal class ValidationPipeHandler<TRequest, TResponse> : IPipeRequestHandler<TRequest, TRequest, IStatusResult<HttpStatusCode, TResponse>, IStatusResult<HttpStatusCode, TResponse>>
    {
        private readonly IEnumerable<IValidator<TRequest>> validators;
        private readonly ILogger<ValidationPipeHandler<TRequest, TResponse>> logger;

        public ValidationPipeHandler(
            IEnumerable<IValidator<TRequest>> validators,
            ILogger<ValidationPipeHandler<TRequest, TResponse>> logger
        )
        {
            this.validators = validators;
            this.logger = logger;
        }

        public async Task<IStatusResult<HttpStatusCode, TResponse>> HandleAsync(
            TRequest request,
            CancellationToken cancellationToken,
            Func<TRequest, Task<IStatusResult<HttpStatusCode, TResponse>>> next
        )
        {
            logger.Trace($"Validate {typeof(TRequest)}");
            var result = Result.Join(await Task.WhenAll(validators.Select(v => v.ValidateAsync(request))));
            if (result.HasErrors)
            {
                logger.Trace($"Validation of {typeof(TRequest)} failed");

                return Result.New(HttpStatusCode.BadRequest, default(TResponse)).Join(result);
            }

            return await next(request);
        }
    }
}