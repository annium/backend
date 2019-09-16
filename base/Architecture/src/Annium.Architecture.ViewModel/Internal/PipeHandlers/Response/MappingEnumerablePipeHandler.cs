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
    internal class MappingEnumerablePipeHandler<TRequest, TResponseIn, TResponseInBase, TResponseOut, TResponseOutBase> : IPipeRequestHandler<TRequest, TRequest, IStatusResult<OperationStatus, TResponseIn>, IStatusResult<OperationStatus, TResponseOut>> where TResponseIn : IEnumerable<TResponseInBase> where TResponseOut : IEnumerable<TResponseOutBase> where TResponseOutBase : IResponse<TResponseInBase>
    {
        private readonly IMapper mapper;
        private readonly ILogger<MappingEnumerablePipeHandler<TRequest, TResponseIn, TResponseInBase, TResponseOut, TResponseOutBase>> logger;

        public MappingEnumerablePipeHandler(
            IMapper mapper,
            ILogger<MappingEnumerablePipeHandler<TRequest, TResponseIn, TResponseInBase, TResponseOut, TResponseOutBase>> logger
        )
        {
            this.mapper = mapper;
            this.logger = logger;
        }

        public async Task<IStatusResult<OperationStatus, TResponseOut>> HandleAsync(
            TRequest request,
            CancellationToken cancellationToken,
            Func<TRequest, Task<IStatusResult<OperationStatus, TResponseIn>>> next
        )
        {
            var response = await next(request);

            logger.Trace($"Map response: {typeof(TResponseIn)} -> {typeof(TResponseOut)}");
            var mappedResponse = mapper.Map<TResponseOut>(response.Data);

            return Result.Status(response.Status, mappedResponse).Join(response);
        }
    }
}