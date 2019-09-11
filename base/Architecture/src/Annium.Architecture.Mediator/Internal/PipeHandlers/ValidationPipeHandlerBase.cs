using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Data.Operations;
using Annium.Extensions.Validation;
using Annium.Logging.Abstractions;

namespace Annium.Architecture.Mediator.Internal.PipeHandlers
{
    internal abstract class ValidationPipeHandlerBase<TRequest, TResponse>
    {
        private readonly IValidator<TRequest> validator;
        private readonly ILogger<ValidationPipeHandlerBase<TRequest, TResponse>> logger;

        public ValidationPipeHandlerBase(
            IValidator<TRequest> validator,
            ILogger<ValidationPipeHandlerBase<TRequest, TResponse>> logger
        )
        {
            this.validator = validator;
            this.logger = logger;
        }

        public async Task<TResponse> HandleAsync(
            TRequest request,
            CancellationToken cancellationToken,
            Func<TRequest, Task<TResponse>> next
        )
        {
            logger.Trace($"Validate {typeof(TRequest)}");
            var result = await validator.ValidateAsync(request);
            if (result.HasErrors)
            {
                logger.Trace($"Validation of {typeof(TRequest)} failed");

                return GetResponse(result);
            }

            return await next(request);
        }

        protected abstract TResponse GetResponse(IResult validationResult);
    }
}