using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Mediator;
using Annium.Data.Operations;
using Microsoft.AspNetCore.Mvc;

namespace Annium.AspNetCore.Extensions
{
    public class ServerController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IServiceProvider _serviceProvider;

        protected ServerController(
            IMediator mediator,
            IServiceProvider serviceProvider
        )
        {
            _mediator = mediator;
            _serviceProvider = serviceProvider;
        }

        [NonAction]
        protected Task<IResult<TResponse>> HandleAsync<TRequest, TResponse>(
            TRequest request,
            CancellationToken ct = default
        )
        {
            return _mediator.SendAsync<IResult<TResponse>>(_serviceProvider, request, ct);
        }

        [NonAction]
        protected Task<IResult> HandleAsync<TRequest>(
            TRequest request,
            CancellationToken ct = default
        )
        {
            return _mediator.SendAsync<IResult>(_serviceProvider, request, ct);
        }
    }
}