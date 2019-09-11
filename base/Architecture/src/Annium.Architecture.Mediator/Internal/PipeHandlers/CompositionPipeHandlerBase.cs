using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Data.Operations;
using Annium.Extensions.Composition;
using Annium.Logging.Abstractions;

namespace Annium.Architecture.Mediator.Internal.PipeHandlers
{
    internal abstract class CompositionPipeHandlerBase<TRequest, TResponse> where TRequest : class
    {
        private readonly IComposer<TRequest> composer;
        private readonly ILogger<CompositionPipeHandlerBase<TRequest, TResponse>> logger;

        public CompositionPipeHandlerBase(
            IComposer<TRequest> composer,
            ILogger<CompositionPipeHandlerBase<TRequest, TResponse>> logger
        )
        {
            this.composer = composer;
            this.logger = logger;
        }

        public async Task<TResponse> HandleAsync(
            TRequest request,
            CancellationToken cancellationToken,
            Func<TRequest, Task<TResponse>> next
        )
        {
            logger.Trace($"Compose {typeof(TRequest)}");
            var result = await composer.ComposeAsync(request);
            if (result.HasErrors)
            {
                logger.Trace($"Composition of {typeof(TRequest)} failed");

                return GetResponse(result);
            }

            return await next(request);
        }

        protected abstract TResponse GetResponse(IStatusResult<OperationStatus> compositionResult);
    }
}