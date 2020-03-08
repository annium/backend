using System;
using Annium.Architecture.Base;
using Annium.Core.Mediator;
using Annium.Data.Operations;
using Annium.Logging.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Annium.AspNetCore.Extensions.Internal.PipeHandlers.Request
{
    internal class ModelStatePipeHandler<TRequest> :
        ModelStatePipeHandlerBase<TRequest, IStatusResult<OperationStatus>>,
        IPipeRequestHandler<ValueTuple<ModelStateDictionary, TRequest>, TRequest, IStatusResult<OperationStatus>, IStatusResult<OperationStatus>>
    {
        public ModelStatePipeHandler(
            ILogger<ModelStatePipeHandlerBase<TRequest, IStatusResult<OperationStatus>>> logger
        ) : base(logger)
        {
        }

        protected override IStatusResult<OperationStatus> GetResponse(IStatusResult<OperationStatus> result)
        {
            return result;
        }
    }
}