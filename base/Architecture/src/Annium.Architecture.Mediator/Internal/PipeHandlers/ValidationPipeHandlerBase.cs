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
        private readonly IValidator<TRequest> _validator;
        private readonly ILogger<ValidationPipeHandlerBase<TRequest, TResponse>> _logger;

        public ValidationPipeHandlerBase(
            IValidator<TRequest> validator,
            ILogger<ValidationPipeHandlerBase<TRequest, TResponse>> logger
        )
        {
            _validator = validator;
            _logger = logger;
        }

        public async Task<TResponse> HandleAsync(
            TRequest request,
            CancellationToken cancellationToken,
            Func<TRequest, Task<TResponse>> next
        )
        {
            _logger.Trace($"Validate {typeof(TRequest)}");
            if (request is null)
            {
                _logger.Trace($"Validation of {typeof(TRequest)} failed - request is null");

                return GetResponse(Result.New().Error("Request is empty"));
            }

            var result = await _validator.ValidateAsync(request);
            if (result.HasErrors)
            {
                _logger.Trace($"Validation of {typeof(TRequest)} failed");

                return GetResponse(result);
            }

            return await next(request);
        }

        protected abstract TResponse GetResponse(IResult validationResult);
    }
}