using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Core.Mediator;
using Annium.Data.Operations;
using Annium.Extensions.Primitives;
using Annium.Logging.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Annium.AspNetCore.Extensions.Internal.PipeHandlers
{
    public class ModelStatePipeHandler<TRequest, TResponse> : IPipeRequestHandler<ValueTuple<ModelStateDictionary, TRequest>, TRequest, IStatusResult<OperationStatus, TResponse>, IStatusResult<OperationStatus, TResponse>>
    {
        private readonly ILogger<ModelStatePipeHandler<TRequest, TResponse>> logger;

        public ModelStatePipeHandler(
            ILogger<ModelStatePipeHandler<TRequest, TResponse>> logger
        )
        {
            this.logger = logger;
        }

        public Task<IStatusResult<OperationStatus, TResponse>> HandleAsync(
            ValueTuple<ModelStateDictionary, TRequest> payload,
            CancellationToken cancellationToken,
            Func<TRequest, Task<IStatusResult<OperationStatus, TResponse>>> next
        )
        {
            var(modelState, request) = payload;
            if (!modelState.IsValid)
            {
                logger.Trace($"Model of {typeof(TRequest).Name} is not valid");
                return Task.FromResult(GetBadRequestResult(modelState));
            }

            return next(request);
        }

        private IStatusResult<OperationStatus, TResponse> GetBadRequestResult(ModelStateDictionary modelState)
        {
            var result = Result.Status<OperationStatus, TResponse>(OperationStatus.BadRequest, default(TResponse));

            foreach (var(field, entry) in modelState)
            {
                var label = field.CamelCase();
                foreach (var error in entry.Errors)
                    result.Error(label, error.ErrorMessage);
            }

            return result;
        }
    }
}