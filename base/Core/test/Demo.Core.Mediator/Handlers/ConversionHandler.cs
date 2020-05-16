using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Mediator;
using Annium.Logging.Abstractions;
using Demo.Core.Mediator.ViewModels;

namespace Demo.Core.Mediator.Handlers
{
    internal class ConversionHandler<TRequest, TResponse> : IPipeRequestHandler<Request<TRequest>, TRequest, TResponse, Response<TResponse>>
    {
        private readonly ILogger<ConversionHandler<TRequest, TResponse>> logger;

        public ConversionHandler(
            ILogger<ConversionHandler<TRequest, TResponse>> logger
        )
        {
            this.logger = logger;
        }

        public async Task<Response<TResponse>> HandleAsync(
            Request<TRequest> request,
            CancellationToken ct,
            Func<TRequest, Task<TResponse>> next
        )
        {
            logger.Trace($"Deserialize Request to {typeof(TRequest).Name}");
            var payload = JsonSerializer.Deserialize<TRequest>(request.Value);

            var result = await next(payload);

            logger.Trace($"Serialize {typeof(TResponse).Name} to Response");
            return new Response<TResponse>(JsonSerializer.Serialize(result));
        }
    }
}