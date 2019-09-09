using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Core.Mediator;
using Annium.Data.Operations;
using Annium.Logging.Abstractions;

namespace Annium.Architecture.Mediator.Internal.PipeHandlers
{
    internal class ExceptionPipeHandler<TRequest, TResponse> : IPipeRequestHandler<TRequest, TRequest, IStatusResult<OperationStatus, TResponse>, IStatusResult<OperationStatus, TResponse>>
    {
        private readonly ILogger<ExceptionPipeHandler<TRequest, TResponse>> logger;

        public ExceptionPipeHandler(
            ILogger<ExceptionPipeHandler<TRequest, TResponse>> logger
        )
        {
            this.logger = logger;
        }

        public async Task<IStatusResult<OperationStatus, TResponse>> HandleAsync(
            TRequest request,
            CancellationToken cancellationToken,
            Func<TRequest, Task<IStatusResult<OperationStatus, TResponse>>> next
        )
        {
            try
            {
                return await next(request);
            }
            catch (TargetInvocationException exception)
            {
                return GetFailure(exception.InnerException);
            }
            catch (Exception exception)
            {
                return GetFailure(exception);
            }
        }

        private IStatusResult<OperationStatus, TResponse> GetFailure(Exception exception)
        {
            logger.Trace($"Failure of {typeof(TRequest)}: {exception}");

            return Result.Status(OperationStatus.UncaughtException, default(TResponse)).Error(exception.Message);
        }
    }
}