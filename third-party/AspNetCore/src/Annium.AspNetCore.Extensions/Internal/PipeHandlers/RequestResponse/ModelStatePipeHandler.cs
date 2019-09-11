using System;
using Annium.Architecture.Base;
using Annium.Core.Mediator;
using Annium.Data.Operations;
using Annium.Logging.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Annium.AspNetCore.Extensions.Internal.PipeHandlers.RequestResponse
{
    internal class ModelStatePipeHandler<TRequest, TResponse> : ModelStatePipeHandlerBase<TRequest, IStatusResult<OperationStatus, TResponse>>, IPipeRequestHandler<ValueTuple<ModelStateDictionary, TRequest>, TRequest, IStatusResult<OperationStatus, TResponse>, IStatusResult<OperationStatus, TResponse>>
    {
        public ModelStatePipeHandler(
            ILogger<ModelStatePipeHandlerBase<TRequest, IStatusResult<OperationStatus, TResponse>>> logger
        ) : base(logger)
        {

        }

        protected override IStatusResult<OperationStatus, TResponse> GetResponse(IStatusResult<OperationStatus> result)
        {
            return Result.Status(result.Status, default(TResponse)).Join(result);
        }
    }
}