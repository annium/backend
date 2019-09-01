using System;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Mediator;
using Annium.Data.Operations;
using Annium.Logging.Abstractions;

namespace Annium.Architecture.Mediator.PipeHandlers
{
    internal class ExceptionPipeHandler<TRequest, TResponse> : IPipeRequestHandler<TRequest, TRequest, IStatusResult<HttpStatusCode, TResponse>, IStatusResult<HttpStatusCode, TResponse>>
    {
        private readonly ILogger<ExceptionPipeHandler<TRequest, TResponse>> logger;

        public ExceptionPipeHandler(
            ILogger<ExceptionPipeHandler<TRequest, TResponse>> logger
        )
        {
            this.logger = logger;
        }

        public async Task<IStatusResult<HttpStatusCode, TResponse>> HandleAsync(
            TRequest request,
            CancellationToken cancellationToken,
            Func<TRequest, Task<IStatusResult<HttpStatusCode, TResponse>>> next
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

        private IStatusResult<HttpStatusCode, TResponse> GetFailure(Exception exception)
        {
            logger.Trace($"Failure of {typeof(TRequest)}: {exception}");

            return Result.New(HttpStatusCode.InternalServerError, default(TResponse)).Error(exception.Message);
        }
    }
}