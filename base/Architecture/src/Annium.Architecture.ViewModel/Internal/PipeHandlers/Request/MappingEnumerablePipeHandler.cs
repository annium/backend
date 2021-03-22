using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Mapper;
using Annium.Core.Mediator;
using Annium.Logging.Abstractions;

namespace Annium.Architecture.ViewModel.Internal.PipeHandlers.Request
{
    internal class MappingEnumerablePipeHandler<TRequestIn, TRequestOut, TResponse> :
        IPipeRequestHandler<
            IEnumerable<TRequestIn>,
            IEnumerable<TRequestOut>,
            TResponse,
            TResponse
        >
        where TRequestIn : IRequest<TRequestOut>
    {
        private readonly IMapper _mapper;
        private readonly ILogger<MappingEnumerablePipeHandler<TRequestIn, TRequestOut, TResponse>> _logger;

        public MappingEnumerablePipeHandler(
            IMapper mapper,
            ILogger<MappingEnumerablePipeHandler<TRequestIn, TRequestOut, TResponse>> logger
        )
        {
            _mapper = mapper;
            _logger = logger;
        }

        public Task<TResponse> HandleAsync(
            IEnumerable<TRequestIn> request,
            CancellationToken ct,
            Func<IEnumerable<TRequestOut>, CancellationToken, Task<TResponse>> next
        )
        {
            _logger.Trace($"Map request: {typeof(TRequestIn)} -> {typeof(TRequestOut)}");
            var mappedRequest = _mapper.Map<IEnumerable<TRequestOut>>(request);

            return next(mappedRequest, ct);
        }
    }
}