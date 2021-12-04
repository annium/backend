using Annium.Architecture.Base;
using Annium.Core.Mediator;
using Annium.Data.Operations;
using Annium.Extensions.Composition;
using Annium.Logging.Abstractions;

namespace Annium.Architecture.Mediator.Internal.PipeHandlers.Request;

internal class CompositionPipeHandler<TRequest> :
    CompositionPipeHandlerBase<TRequest, IStatusResult<OperationStatus>>,
    IPipeRequestHandler<TRequest, TRequest, IStatusResult<OperationStatus>, IStatusResult<OperationStatus>> where TRequest : class
{
    public CompositionPipeHandler(
        IComposer<TRequest> composer,
        ILogger<CompositionPipeHandlerBase<TRequest, IStatusResult<OperationStatus>>> logger
    ) : base(composer, logger)
    {
    }

    protected override IStatusResult<OperationStatus> GetResponse(IStatusResult<OperationStatus> compositionResult)
    {
        return compositionResult;
    }
}