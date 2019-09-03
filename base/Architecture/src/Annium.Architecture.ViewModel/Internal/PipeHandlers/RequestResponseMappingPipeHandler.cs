using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Core.Mapper;
using Annium.Core.Mediator;
using Annium.Data.Operations;
using Annium.Logging.Abstractions;

namespace Annium.Architecture.ViewModel.Internal.PipeHandlers
{
    internal class RequestResponseMappingPipeHandler<TRequestIn, TRequestOut, TResponseIn, TResponseOut> : IPipeRequestHandler<TRequestIn, TRequestOut, IStatusResult<OperationStatus, TResponseIn>, IStatusResult<OperationStatus, TResponseOut>> where TRequestIn : IRequest<TRequestOut> where TResponseOut : IResponse<TResponseIn>
    {
        private readonly IMapper mapper;
        private readonly ILogger<RequestResponseMappingPipeHandler<TRequestIn, TRequestOut, TResponseIn, TResponseOut>> logger;

        public RequestResponseMappingPipeHandler(
            IMapper mapper,
            ILogger<RequestResponseMappingPipeHandler<TRequestIn, TRequestOut, TResponseIn, TResponseOut>> logger
        )
        {
            this.mapper = mapper;
            this.logger = logger;
        }

        public async Task<IStatusResult<OperationStatus, TResponseOut>> HandleAsync(
            TRequestIn request,
            CancellationToken cancellationToken,
            Func<TRequestOut, Task<IStatusResult<OperationStatus, TResponseIn>>> next
        )
        {
            logger.Trace($"Map request: {typeof(TRequestIn)} -> {typeof(TRequestOut)}");
            var mappedRequest = mapper.Map<TRequestOut>(request);

            var response = await next(mappedRequest);

            logger.Trace($"Map response: {typeof(TResponseIn)} -> {typeof(TResponseOut)}");
            var mappedResponse = mapper.Map<TResponseOut>(response.Data);

            return Result.New(response.Status, mappedResponse).Join(response);
        }
    }
}