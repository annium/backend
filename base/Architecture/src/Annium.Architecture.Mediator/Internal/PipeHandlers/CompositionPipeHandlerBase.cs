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
        private readonly IComposer<TRequest> _composer;
        private readonly ILogger<CompositionPipeHandlerBase<TRequest, TResponse>> _logger;

        public CompositionPipeHandlerBase(
            IComposer<TRequest> composer,
            ILogger<CompositionPipeHandlerBase<TRequest, TResponse>> logger
        )
        {
            _composer = composer;
            _logger = logger;
        }

        public async Task<TResponse> HandleAsync(
            TRequest request,
            CancellationToken cancellationToken,
            Func<TRequest, Task<TResponse>> next
        )
        {
            _logger.Trace($"Compose {typeof(TRequest)}");
            var result = await _composer.ComposeAsync(request);
            if (result.HasErrors)
            {
                _logger.Trace($"Composition of {typeof(TRequest)} failed");

                return GetResponse(result);
            }

            return await next(request);
        }

        protected abstract TResponse GetResponse(IStatusResult<OperationStatus> compositionResult);
    }
}