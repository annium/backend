using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Mediator;
using Annium.Data.Operations;
using Annium.Extensions.Validation;
using Annium.Logging.Abstractions;

namespace Demo.Core.Mediator.Handlers
{
    internal class ValidationHandler<TRequest, TResponse> : IPipeRequestHandler<TRequest, TRequest, TResponse, IBooleanResult<TResponse>>
    {
        private readonly IEnumerable<IValidator<TRequest>> validators;
        private readonly ILogger<ValidationHandler<TRequest, TResponse>> logger;

        public ValidationHandler(
            IEnumerable<IValidator<TRequest>> validators,
            ILogger<ValidationHandler<TRequest, TResponse>> logger
        )
        {
            this.validators = validators;
            this.logger = logger;
        }

        public async Task<IBooleanResult<TResponse>> HandleAsync(
            TRequest request,
            CancellationToken ct,
            Func<TRequest, Task<TResponse>> next
        )
        {
            logger.Trace($"Start {typeof(TRequest).Name} validation");
            var result = Result.Failure(default(TResponse) !)
                .Join(await Task.WhenAll(validators.Select(v => v.ValidateAsync(request))));
            logger.Trace($"Status of {typeof(TRequest).Name} validation: {result.IsFailure}");
            if (result.HasErrors)
                return result;

            var response = await next(request);

            return Result.Success(response);
        }
    }
}