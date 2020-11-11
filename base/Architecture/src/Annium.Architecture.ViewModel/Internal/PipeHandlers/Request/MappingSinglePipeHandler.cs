using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Mapper;
using Annium.Core.Mediator;
using Annium.Logging.Abstractions;

namespace Annium.Architecture.ViewModel.Internal.PipeHandlers.Request
{
    internal class MappingSinglePipeHandler<TRequestIn, TRequestOut, TResponse> :
        IPipeRequestHandler<
            TRequestIn,
            TRequestOut,
            TResponse,
            TResponse
        >
        where TRequestIn : IRequest<TRequestOut>
    {
        private readonly IMapper _mapper;
        private readonly ILogger<MappingSinglePipeHandler<TRequestIn, TRequestOut, TResponse>> _logger;

        public MappingSinglePipeHandler(
            IMapper mapper,
            ILogger<MappingSinglePipeHandler<TRequestIn, TRequestOut, TResponse>> logger
        )
        {
            _mapper = mapper;
            _logger = logger;
        }

        public Task<TResponse> HandleAsync(
            TRequestIn request,
            CancellationToken ct,
            Func<TRequestOut, Task<TResponse>> next
        )
        {
            _logger.Trace($"Map request: {typeof(TRequestIn)} -> {typeof(TRequestOut)}");
            var mappedRequest = _mapper.Map<TRequestOut>(request);

            return next(mappedRequest);
        }
    }
}