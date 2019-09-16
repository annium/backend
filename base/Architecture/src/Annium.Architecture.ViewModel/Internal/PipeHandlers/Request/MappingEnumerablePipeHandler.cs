using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Mapper;
using Annium.Core.Mediator;
using Annium.Logging.Abstractions;

namespace Annium.Architecture.ViewModel.Internal.PipeHandlers.Request
{
    internal class MappingEnumerablePipeHandler<TRequestIn, TRequestInBase, TRequestOut, TRequestOutBase, TResponse> : IPipeRequestHandler<TRequestIn, TRequestOut, TResponse, TResponse> where TRequestIn : IEnumerable<TRequestInBase> where TRequestOut : IEnumerable<TRequestOutBase> where TRequestInBase : IRequest<TRequestOutBase>
    {
        private readonly IMapper mapper;
        private readonly ILogger<MappingEnumerablePipeHandler<TRequestIn, TRequestInBase, TRequestOut, TRequestOutBase, TResponse>> logger;

        public MappingEnumerablePipeHandler(
            IMapper mapper,
            ILogger<MappingEnumerablePipeHandler<TRequestIn, TRequestInBase, TRequestOut, TRequestOutBase, TResponse>> logger
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