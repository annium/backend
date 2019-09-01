using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Mediator;
using Annium.Logging.Abstractions;

namespace Annium.Architecture.Mediator.PipeHandlers
{
    internal class LoggingPipeHandler<TRequest, TResponse> : IPipeRequestHandler<TRequest, TRequest, TResponse, TResponse>
    {
        private readonly ILogger<LoggingPipeHandler<TRequest, TResponse>> logger;

        public LoggingPipeHandler(
            ILogger<LoggingPipeHandler<TRequest, TResponse>> logger
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
            logger.Trace($"Start {typeof(TRequest)} -> {typeof(TResponse)}");

            var result = await next(request);

            logger.Trace($"Complete {typeof(TRequest)} -> {typeof(TResponse)}");

            return result;
        }
    }
}