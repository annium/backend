using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging.Abstractions;

namespace Annium.Architecture.Mediator.Internal.PipeHandlers
{
    internal abstract class ExceptionPipeHandlerBase<TRequest, TResponse>
    {
        private readonly ILogger<ExceptionPipeHandlerBase<TRequest, TResponse>> logger;

        public ExceptionPipeHandlerBase(
            ILogger<ExceptionPipeHandlerBase<TRequest, TResponse>> logger
        )
        {
            this.logger = logger;
        }

        public async Task<TResponse> HandleAsync(
            TRequest request,
            CancellationToken cancellationToken,
            Func<TRequest, Task<TResponse>> next
        )
        {
            try
            {
                return await next(request);
            }
            catch (TargetInvocationException exception)
            {
                return Failure(exception.InnerException);
            }
            catch (Exception exception)
            {
                return Failure(exception);
            }
        }

        protected abstract TResponse GetFailure(Exception exception);

        private TResponse Failure(Exception exception)
        {
            logger.Trace($"Failure of {typeof(TRequest)}: {exception}");

            return GetFailure(exception);
        }
    }
}