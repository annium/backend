using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Core.Mediator;
using Annium.Data.Operations;
using Annium.Extensions.Validation;
using Annium.Logging.Abstractions;

namespace Annium.Architecture.Mediator.Internal.PipeHandlers
{
    internal class ValidationPipeHandler<TRequest, TResponse> : IPipeRequestHandler<TRequest, TRequest, IStatusResult<OperationStatus, TResponse>, IStatusResult<OperationStatus, TResponse>>
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

        public async Task<IStatusResult<OperationStatus, TResponse>> HandleAsync(
            TRequest request,
            CancellationToken cancellationToken,
            Func<TRequest, Task<IStatusResult<OperationStatus, TResponse>>> next
        )
        {
            logger.Trace($"Validate {typeof(TRequest)}");
            var result = Result.Join(await Task.WhenAll(validators.Select(v => v.ValidateAsync(request))));
            if (result.HasErrors)
            {
                logger.Trace($"Validation of {typeof(TRequest)} failed");

                return Result.New(OperationStatus.BadRequest, default(TResponse)).Join(result);
            }

            return await next(request);
        }
    }
}