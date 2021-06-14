using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Mediator;
using Annium.Logging.Abstractions;
using Demo.Core.Mediator.ViewModels;

namespace Demo.Core.Mediator.Handlers
{
    internal class ConversionHandler<TRequest, TResponse> :
        IPipeRequestHandler<Request<TRequest>, TRequest, TResponse, Response<TResponse>>,
        ILogSubject
    {
        public ILogger Logger { get; }

        public ConversionHandler(
            ILogger<ConversionHandler<TRequest, TResponse>> logger
        )
        {
            Logger = logger;
        }

        public async Task<Response<TResponse>> HandleAsync(
            Request<TRequest> request,
            CancellationToken ct,
            Func<TRequest, CancellationToken, Task<TResponse>> next
        )
        {
            this.Log().Trace($"Deserialize Request to {typeof(TRequest).Name}");
            var payload = JsonSerializer.Deserialize<TRequest>(request.Value)!;

            var result = await next(payload, ct);

            this.Log().Trace($"Serialize {typeof(TResponse).Name} to Response");
            return new Response<TResponse>(JsonSerializer.Serialize(result));
        }
    }
}