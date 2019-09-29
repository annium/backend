using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Core.Mapper;
using Annium.Core.Mediator;
using Annium.Data.Operations;
using Annium.Logging.Abstractions;

namespace Annium.Architecture.ViewModel.Internal.PipeHandlers.Response
{
    internal class MappingSinglePipeHandler<TRequest, TResponseIn, TResponseOut> : IPipeRequestHandler<TRequest, TRequest, IStatusResult<OperationStatus, TResponseIn>, IStatusResult<OperationStatus, TResponseOut>> where TResponseOut : IResponse<TResponseIn>
    {
        private readonly IMapper mapper;
        private readonly ILogger<MappingSinglePipeHandler<TRequest, TResponseIn, TResponseOut>> logger;

        public MappingSinglePipeHandler(
            IMapper mapper,
            ILogger<MappingSinglePipeHandler<TRequest, TResponseIn, TResponseOut>> logger
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
            var mappedResponse = mapper.Map<TResponseOut>(response.Data!);

            return Result.Status(response.Status, mappedResponse).Join(response);
        }
    }
}