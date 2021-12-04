using Annium.Architecture.Base;
using Annium.Core.Mediator;
using Annium.Data.Operations;
using Annium.Extensions.Validation;
using Annium.Logging.Abstractions;

namespace Annium.Architecture.Mediator.Internal.PipeHandlers.Request;

internal class ValidationPipeHandler<TRequest> : ValidationPipeHandlerBase<TRequest, IStatusResult<OperationStatus>>,
    IPipeRequestHandler<TRequest, TRequest, IStatusResult<OperationStatus>, IStatusResult<OperationStatus>>
{
    public ValidationPipeHandler(
        IValidator<TRequest> validator,
        ILogger<ValidationPipeHandlerBase<TRequest, IStatusResult<OperationStatus>>> logger
    ) : base(validator, logger)
    {
    }

    protected override IStatusResult<OperationStatus> GetResponse(IResult validationResult)
    {
        return Result.Status(OperationStatus.BadRequest).Join(validationResult);
    }
}