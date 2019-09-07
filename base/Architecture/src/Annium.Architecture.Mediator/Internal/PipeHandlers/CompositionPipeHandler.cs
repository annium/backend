using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Core.Mediator;
using Annium.Data.Operations;
using Annium.Extensions.Composition;
using Annium.Logging.Abstractions;

namespace Annium.Architecture.Mediator.Internal.PipeHandlers
{
    internal class CompositionPipeHandler<TRequest, TResponse> : IPipeRequestHandler<TRequest, TRequest, IStatusResult<OperationStatus, TResponse>, IStatusResult<OperationStatus, TResponse>> where TRequest : class
    {
        private readonly IComposer<TRequest> composer;
        private readonly ILogger<CompositionPipeHandler<TRequest, TResponse>> logger;

        public CompositionPipeHandler(
            IComposer<TRequest> composer,
            ILogger<CompositionPipeHandler<TRequest, TResponse>> logger
        )
        {
            this.composer = composer;
            this.logger = logger;
        }

        public async Task<IStatusResult<OperationStatus, TResponse>> HandleAsync(
            TRequest request,
            CancellationToken cancellationToken,
            Func<TRequest, Task<IStatusResult<OperationStatus, TResponse>>> next
        )
        {
            logger.Trace($"Compose {typeof(TRequest)}");
            var result = await composer.ComposeAsync(request);
            if (result.HasErrors)
            {
                logger.Trace($"Composition of {typeof(TRequest)} failed");

                return Result.New(result.Status, default(TResponse)).Join(result);
            }

            return await next(request);
        }
    }
}