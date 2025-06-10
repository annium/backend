using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Mediator;
using Annium.Data.Operations;
using Microsoft.AspNetCore.Mvc;

namespace Annium.AspNetCore.Extensions.Controllers;

/// <summary>
/// Base controller that provides mediator-based request handling capabilities for ASP.NET Core controllers
/// </summary>
public class ServerController : ControllerBase
{
    /// <summary>
    /// The mediator for handling requests
    /// </summary>
    private readonly IMediator _mediator;

    /// <summary>
    /// The service provider for dependency resolution
    /// </summary>
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the ServerController class
    /// </summary>
    /// <param name="mediator">The mediator for handling requests</param>
    /// <param name="serviceProvider">The service provider for dependency resolution</param>
    protected ServerController(IMediator mediator, IServiceProvider serviceProvider)
    {
        _mediator = mediator;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Handles a request with a response through the mediator
    /// </summary>
    /// <typeparam name="TRequest">The type of the request</typeparam>
    /// <typeparam name="TResponse">The type of the response</typeparam>
    /// <param name="request">The request to handle</param>
    /// <param name="ct">The cancellation token</param>
    /// <returns>A task that represents the asynchronous operation containing the result with response</returns>
    [NonAction]
    protected Task<IResult<TResponse>> HandleAsync<TRequest, TResponse>(
        TRequest request,
        CancellationToken ct = default
    )
        where TRequest : notnull
    {
        return _mediator.SendAsync<IResult<TResponse>>(_serviceProvider, request, ct);
    }

    /// <summary>
    /// Handles a request without a response through the mediator
    /// </summary>
    /// <typeparam name="TRequest">The type of the request</typeparam>
    /// <param name="request">The request to handle</param>
    /// <param name="ct">The cancellation token</param>
    /// <returns>A task that represents the asynchronous operation containing the result</returns>
    [NonAction]
    protected Task<IResult> HandleAsync<TRequest>(TRequest request, CancellationToken ct = default)
        where TRequest : notnull
    {
        return _mediator.SendAsync<IResult>(_serviceProvider, request, ct);
    }
}
