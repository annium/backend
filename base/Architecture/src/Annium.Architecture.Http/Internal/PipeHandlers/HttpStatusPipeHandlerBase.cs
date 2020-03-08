using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Architecture.Http.Exceptions;
using Annium.Data.Operations;

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

        protected void HandleStatus(OperationStatus status, IResultBase result)
        {
            if (status == OperationStatus.BadRequest)
                throw new ValidationException(result);

            if (status == OperationStatus.Forbidden)
                throw new ForbiddenException(result);

            if (status == OperationStatus.NotFound)
                throw new NotFoundException(result);

            if (status == OperationStatus.Conflict)
                throw new ConflictException(result);

            if (status == OperationStatus.UncaughtException)
                throw new ServerException(result);

            // if mapping fails - it's critical error
            if (status != OperationStatus.OK)
                throw new ServerException(result);
        }
    }
}