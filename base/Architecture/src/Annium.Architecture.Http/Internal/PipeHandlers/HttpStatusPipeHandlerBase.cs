using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Annium.Architecture.Base;

namespace Annium.Architecture.Http.Internal.PipeHandlers
{
    internal abstract class HttpStatusPipeHandlerBase<TRequest, TResponseIn, TResponseOut>
    {
        public async Task<TResponseOut> HandleAsync(
            TRequest request,
            CancellationToken cancellationToken,
            Func<TRequest, Task<TResponseIn>> next
        )
        {
            var response = await next(request);

            return GetResponse(response);
        }

        protected abstract TResponseOut GetResponse(TResponseIn response);

        protected HttpStatusCode MapToStatusCode(OperationStatus status)
        {
            if (status == OperationStatus.BadRequest)
                return HttpStatusCode.BadRequest;

            if (status == OperationStatus.Forbidden)
                return HttpStatusCode.Forbidden;

            if (status == OperationStatus.OK)
                return HttpStatusCode.OK;

            if (status == OperationStatus.NotFound)
                return HttpStatusCode.NotFound;

            if (status == OperationStatus.UncaughtException)
                return HttpStatusCode.InternalServerError;

            // if mapping fails - it's critical error
            return HttpStatusCode.InternalServerError;
        }
    }
}