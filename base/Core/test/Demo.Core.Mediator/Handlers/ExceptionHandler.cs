using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Mediator;
using Annium.Data.Operations;
using Annium.Logging.Abstractions;

namespace Demo.Core.Mediator.Handlers
{
    internal class ExceptionHandler<TRequest, TResponse> : IPipeRequestHandler<TRequest, TRequest, BooleanResult<TResponse>, BooleanResult<TResponse>>
    {
        private readonly ILogger<LoggingHandler<TRequest, TResponse>> logger;

        public ExceptionHandler(
            ILogger<LoggingHandler<TRequest, TResponse>> logger
        )
        {
            this.logger = logger;
        }

        public async Task<BooleanResult<TResponse>> HandleAsync(
            TRequest request,
            CancellationToken cancellationToken,
            Func<TRequest, Task<BooleanResult<TResponse>>> next
        )
        {
            try
            {
                var result = await next(request);
                logger.Trace($"Request {typeof(TRequest).Name} complete without errors");

                return result;
            }
            catch (Exception exception)
            {
                logger.Trace($"Request {typeof(TRequest).Name} failed with {exception}");
                return Result.Failure<TResponse>(default(TResponse)).Error(exception.Message);
            }
        }
    }
}