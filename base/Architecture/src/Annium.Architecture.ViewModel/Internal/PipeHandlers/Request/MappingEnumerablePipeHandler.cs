using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Mapper;
using Annium.Core.Mediator;
using Annium.Logging.Abstractions;

namespace Annium.Architecture.ViewModel.Internal.PipeHandlers.Request
{
    internal class MappingEnumerablePipeHandler<TRequestIn, TRequestOut, TResponse> : IPipeRequestHandler<IEnumerable<TRequestIn>, IEnumerable<TRequestOut>, TResponse, TResponse> where TRequestIn : IRequest<TRequestOut>
    {
        private readonly IMapper mapper;
        private readonly ILogger<MappingEnumerablePipeHandler<TRequestIn, TRequestOut, TResponse>> logger;

        public MappingEnumerablePipeHandler(
            IMapper mapper,
            ILogger<MappingEnumerablePipeHandler<TRequestIn, TRequestOut, TResponse>> logger
        )
        {
            this.mapper = mapper;
            this.logger = logger;
        }

        public Task<TResponse> HandleAsync(
            IEnumerable<TRequestIn> request,
            CancellationToken cancellationToken,
            Func<IEnumerable<TRequestOut>, Task<TResponse>> next
        )
        {
            logger.Trace($"Map request: {typeof(TRequestIn)} -> {typeof(TRequestOut)}");
            var mappedRequest = mapper.Map<IEnumerable<TRequestOut>>(request);

            return next(mappedRequest);
        }
    }
}