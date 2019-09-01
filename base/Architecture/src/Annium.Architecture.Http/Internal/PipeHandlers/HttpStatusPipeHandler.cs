using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Core.Mediator;
using Annium.Data.Operations;
using Annium.Logging.Abstractions;

namespace Annium.Architecture.Http.Internal.PipeHandlers
{
    public class HttpStatusPipeHandler<TRequest, TResponse> : IPipeRequestHandler<TRequest, TRequest, IStatusResult<OperationStatus, TResponse>, IStatusResult<HttpStatusCode, TResponse>>
    {
        private readonly ILogger<HttpStatusPipeHandler<TRequest, TResponse>> logger;

        public HttpStatusPipeHandler(
            ILogger<HttpStatusPipeHandler<TRequest, TResponse>> logger
        )
        {
            this.logger = logger;
        }

        public async Task<IStatusResult<HttpStatusCode, TResponse>> HandleAsync(
            TRequest request,
            CancellationToken cancellationToken,
            Func<TRequest, Task<IStatusResult<OperationStatus, TResponse>>> next
        )
        {
            var response = await next(request);

            return Result.New(MapToStatusCode(response.Status), response.Data).Join(response);
        }

        private HttpStatusCode MapToStatusCode(OperationStatus status)
        {
            if (status == OperationStatus.OK)
                return HttpStatusCode.OK;

            // if mapping fails - it's critical error
            return HttpStatusCode.InternalServerError;
        }
    }
}