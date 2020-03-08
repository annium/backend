using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Data.Operations;
using Annium.Extensions.Primitives;
using Annium.Logging.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Annium.AspNetCore.Extensions.Internal.PipeHandlers
{
    internal abstract class ModelStatePipeHandlerBase<TRequest, TResponse>
    {
        private readonly ILogger<ModelStatePipeHandlerBase<TRequest, TResponse>> logger;

        public ModelStatePipeHandlerBase(
            ILogger<ModelStatePipeHandlerBase<TRequest, TResponse>> logger
        )
        {
            this.logger = logger;
        }

        public Task<TResponse> HandleAsync(
            ValueTuple<ModelStateDictionary, TRequest> payload,
            CancellationToken cancellationToken,
            Func<TRequest, Task<TResponse>> next
        )
        {
            var (modelState, request) = payload;
            if (!modelState.IsValid)
            {
                logger.Trace($"Model of {typeof(TRequest).Name} is not valid");
                return Task.FromResult(GetResponse(GetBadRequestResult(modelState)));
            }

            return next(request);
        }

        protected abstract TResponse GetResponse(IStatusResult<OperationStatus> result);

        private IStatusResult<OperationStatus> GetBadRequestResult(ModelStateDictionary modelState)
        {
            var result = Result.Status(OperationStatus.BadRequest);

            foreach (var (field, entry) in modelState)
            {
                var label = field.CamelCase();
                foreach (var error in entry.Errors)
                    result.Error(label, error.ErrorMessage);
            }

            return result;
        }
    }
}