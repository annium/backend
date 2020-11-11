using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Mediator;
using Annium.Logging.Abstractions;
using Demo.Core.Mediator.Models;

namespace Demo.Core.Mediator.Handlers
{
    internal class AuthorizationHandler<TRequest, TResponse> : IPipeRequestHandler<TRequest, Authored<TRequest>, TResponse, TResponse>
    {
        private readonly ILogger<AuthorizationHandler<TRequest, TResponse>> _logger;

        public AuthorizationHandler(
            ILogger<AuthorizationHandler<TRequest, TResponse>> logger
        )
        {
            _logger = logger;
        }

        public async Task<TResponse> HandleAsync(
            TRequest request,
            CancellationToken ct,
            Func<Authored<TRequest>, Task<TResponse>> next
        )
        {
            _logger.Trace($"Start {typeof(TRequest).Name} authorization");
            var authoredRequest = new Authored<TRequest>(1, request);

            var response = await next(authoredRequest);

            return response;
        }
    }
}