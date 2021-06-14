using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Mediator;
using Annium.Logging.Abstractions;
using Demo.Core.Mediator.Models;

namespace Demo.Core.Mediator.Handlers
{
    internal class AuthorizationHandler<TRequest, TResponse> :
        IPipeRequestHandler<TRequest, Authored<TRequest>, TResponse, TResponse>,
        ILogSubject
    {
        public ILogger Logger { get; }

        public AuthorizationHandler(
            ILogger<AuthorizationHandler<TRequest, TResponse>> logger
        )
        {
            Logger = logger;
        }

        public async Task<TResponse> HandleAsync(
            TRequest request,
            CancellationToken ct,
            Func<Authored<TRequest>, CancellationToken, Task<TResponse>> next
        )
        {
            this.Trace($"Start {typeof(TRequest).Name} authorization");
            var authoredRequest = new Authored<TRequest>(1, request);

            var response = await next(authoredRequest, ct);

            return response;
        }
    }
}