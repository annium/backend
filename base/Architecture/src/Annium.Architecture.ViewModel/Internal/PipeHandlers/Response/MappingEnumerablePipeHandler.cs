using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Core.Mapper;
using Annium.Core.Mediator;
using Annium.Data.Operations;
using Annium.Logging.Abstractions;

namespace Annium.Architecture.ViewModel.Internal.PipeHandlers.Response
{
    internal class MappingEnumerablePipeHandler<TRequest, TResponseIn, TResponseOut> :
        IPipeRequestHandler<
            TRequest,
            TRequest,
            IStatusResult<OperationStatus, IEnumerable<TResponseIn>>,
            IStatusResult<OperationStatus, IEnumerable<TResponseOut>>
        >
        where TResponseOut : IResponse<TResponseIn>
    {
        private readonly IMapper _mapper;
        private readonly ILogger<MappingEnumerablePipeHandler<TRequest, TResponseIn, TResponseOut>> _logger;

        public MappingEnumerablePipeHandler(
            IMapper mapper,
            ILogger<MappingEnumerablePipeHandler<TRequest, TResponseIn, TResponseOut>> logger
        )
        {
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IStatusResult<OperationStatus, IEnumerable<TResponseOut>>> HandleAsync(
            TRequest request,
            CancellationToken ct,
            Func<TRequest, CancellationToken, Task<IStatusResult<OperationStatus, IEnumerable<TResponseIn>>>> next
        )
        {
            var response = await next(request, ct);

            _logger.Trace($"Map response: {typeof(TResponseIn)} -> {typeof(TResponseOut)}");
            var mappedResponse = _mapper.Map<IEnumerable<TResponseOut>>(response.Data);

            return Result.Status(response.Status, mappedResponse).Join(response);
        }
    }
}