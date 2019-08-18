using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Mediator;
using Annium.Logging.Abstractions;

namespace Demo.Core.Mediator.Handlers
{
    internal class LoggingHandler<TRequest, TResponse> : IPipeRequestHandler<TRequest, TRequest, TResponse, TResponse>
    {
        private readonly ILogger<LoggingHandler<TRequest, TResponse>> logger;

        public LoggingHandler(
            ILogger<LoggingHandler<TRequest, TResponse>> logger
        )
        {
            this.logger = logger;
        }

        public async Task<TResponse> HandleAsync(
            TRequest request,
            CancellationToken cancellationToken,
            Func<TRequest, Task<TResponse>> next
        )
        {
            logger.Trace($"Start {typeof(TRequest).Name} handle");
            var result = await next(request);
            logger.Trace($"Complete {typeof(TRequest).Name} handle");

            return result;
        }
    }
}