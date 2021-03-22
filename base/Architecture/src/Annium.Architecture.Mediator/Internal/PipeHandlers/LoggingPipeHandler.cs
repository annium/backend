using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Mediator;
using Annium.Logging.Abstractions;

namespace Annium.Architecture.Mediator.Internal.PipeHandlers
{
    internal class LoggingPipeHandler<TRequest, TResponse> : IPipeRequestHandler<TRequest, TRequest, TResponse, TResponse>
    {
        private readonly ILogger<LoggingPipeHandler<TRequest, TResponse>> _logger;

        public LoggingPipeHandler(
            ILogger<LoggingPipeHandler<TRequest, TResponse>> logger
        )
        {
            _logger = logger;
        }

        public async Task<TResponse> HandleAsync(
            TRequest request,
            CancellationToken ct,
            Func<TRequest, CancellationToken, Task<TResponse>> next
        )
        {
            _logger.Trace($"Start {typeof(TRequest)} -> {typeof(TResponse)}");

            var result = await next(request, ct);

            _logger.Trace($"Complete {typeof(TRequest)} -> {typeof(TResponse)}");

            return result;
        }
    }
}