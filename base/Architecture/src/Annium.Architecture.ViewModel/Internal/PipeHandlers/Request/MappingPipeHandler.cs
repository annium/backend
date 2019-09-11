using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Mapper;
using Annium.Core.Mediator;
using Annium.Logging.Abstractions;

namespace Annium.Architecture.ViewModel.Internal.PipeHandlers.Request
{
    internal class MappingPipeHandler<TRequestIn, TRequestOut, TResponse> : IPipeRequestHandler<TRequestIn, TRequestOut, TResponse, TResponse> where TRequestIn : IRequest<TRequestOut>
    {
        private readonly IMapper mapper;
        private readonly ILogger<MappingPipeHandler<TRequestIn, TRequestOut, TResponse>> logger;

        public MappingPipeHandler(
            IMapper mapper,
            ILogger<MappingPipeHandler<TRequestIn, TRequestOut, TResponse>> logger
        )
        {
            this.mapper = mapper;
            this.logger = logger;
        }

        public Task<TResponse> HandleAsync(
            TRequestIn request,
            CancellationToken cancellationToken,
            Func<TRequestOut, Task<TResponse>> next
        )
        {
            logger.Trace($"Map request: {typeof(TRequestIn)} -> {typeof(TRequestOut)}");
            var mappedRequest = mapper.Map<TRequestOut>(request);

            return next(mappedRequest);
        }
    }
}