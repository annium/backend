using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Core.Mediator;
using Annium.Data.Operations;

namespace Annium.Architecture.Mediator.Tests
{
    internal class EchoRequestHandler<TRequest> : IFinalRequestHandler<TRequest, IStatusResult<OperationStatus, TRequest>> where TRequest : IThrowing
    {
        public Task<IStatusResult<OperationStatus, TRequest>> HandleAsync(
            TRequest request,
            CancellationToken cancellationToken
        )
        {
            if (request.Throw)
                throw new InvalidOperationException("TEST EXCEPTION");

            return Task.FromResult(Result.New(OperationStatus.OK, request));
        }
    }

    internal interface IThrowing
    {
        bool Throw { get; }
    }
}