using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Mapper;
using Annium.Core.Mediator;
using Annium.Logging.Abstractions;

namespace Annium.Architecture.ViewModel.Internal.PipeHandlers.Request
{
    internal class MappingSinglePipeHandler<TRequestIn, TRequestOut, TResponse> : IPipeRequestHandler<TRequestIn, TRequestOut, TResponse, TResponse> where TRequestIn : IRequest<TRequestOut>
    {
        private readonly IMapper mapper;
        private readonly ILogger<MappingSinglePipeHandler<TRequestIn, TRequestOut, TResponse>> logger;

        public MappingSinglePipeHandler(
            IMapper mapper,
            ILogger<MappingSinglePipeHandler<TRequestIn, TRequestOut, TResponse>> logger
        )
        {
            this.mapper = mapper;
            this.logger = logger;
        }

        public Task<TResponse> HandleAsync(
            TRequestIn request,
            CancellationToken ct,
            Func<TRequestOut, Task<TResponse>> next
        )
        {
            logger.Trace($"Map request: {typeof(TRequestIn)} -> {typeof(TRequestOut)}");
            var mappedRequest = mapper.Map<TRequestOut>(request);

            return next(mappedRequest);
        }
    }
}