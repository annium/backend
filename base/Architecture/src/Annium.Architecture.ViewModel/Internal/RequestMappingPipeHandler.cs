using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Mapper;
using Annium.Core.Mediator;
using Annium.Logging.Abstractions;

namespace Annium.Architecture.ViewModel.Internal
{
    internal class RequestMappingPipeHandler<TRequest, TResponse> : IPipeRequestHandler<IRequest<TRequest>, TRequest, TResponse, TResponse>
    {
        private readonly IMapper mapper;
        private readonly ILogger<RequestMappingPipeHandler<TRequest, TResponse>> logger;

        public RequestMappingPipeHandler(
            IMapper mapper,
            ILogger<RequestMappingPipeHandler<TRequest, TResponse>> logger
        )
        {
            this.mapper = mapper;
            this.logger = logger;
        }

        public Task<TResponse> HandleAsync(
            IRequest<TRequest> request,
            CancellationToken cancellationToken,
            Func<TRequest, Task<TResponse>> next
        )
        {
            logger.Trace($"Map request: {request?.GetType() ?? typeof(IRequest<TRequest>)} -> {typeof(TResponse)}");
            var mappedRequest = mapper.Map<TRequest>(request);

            return next(mappedRequest);
        }
    }
}