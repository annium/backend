using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Mapper;
using Annium.Core.Mediator;
using Annium.Logging.Abstractions;

namespace Annium.Architecture.ViewModel.Internal.PipeHandlers
{
    internal class RequestResponseMappingPipeHandler<TRequest, TResponse> : IPipeRequestHandler<IRequest<TRequest>, TRequest, TResponse, IResponse<TResponse>>
    {
        private readonly IMapper mapper;
        private readonly ILogger<RequestResponseMappingPipeHandler<TRequest, TResponse>> logger;

        public RequestResponseMappingPipeHandler(
            IMapper mapper,
            ILogger<RequestResponseMappingPipeHandler<TRequest, TResponse>> logger
        )
        {
            this.mapper = mapper;
            this.logger = logger;
        }

        public async Task<IResponse<TResponse>> HandleAsync(
            IRequest<TRequest> request,
            CancellationToken cancellationToken,
            Func<TRequest, Task<TResponse>> next
        )
        {
            logger.Trace($"Map request: {request?.GetType() ?? typeof(IRequest<TRequest>)} -> {typeof(TResponse)}");
            var mappedRequest = mapper.Map<TRequest>(request);

            var response = await next(mappedRequest);

            logger.Trace($"Map response: {response?.GetType() ?? typeof(TResponse)} -> {typeof(IResponse<TResponse>)}");
            var mappedResponse = mapper.Map<IResponse<TResponse>>(response);

            return mappedResponse;
        }
    }
}