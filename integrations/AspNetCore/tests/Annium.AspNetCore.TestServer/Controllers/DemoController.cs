using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Architecture.CQRS.Commands;
using Annium.Architecture.CQRS.Queries;
using Annium.AspNetCore.Extensions;
using Annium.AspNetCore.TestServer.Components;
using Annium.Core.Mediator;
using Annium.Data.Operations;
using Microsoft.AspNetCore.Mvc;

namespace Annium.AspNetCore.TestServer.Controllers;

/// <summary>
/// Main controller for handling demo requests and providing test endpoints
/// </summary>
[Route("/")]
public class IndexController : ServerController
{
    /// <summary>
    /// Container for shared data across controller requests
    /// </summary>
    private readonly SharedDataContainer _sharedDataContainer;

    /// <summary>
    /// Initializes a new instance of the IndexController class
    /// </summary>
    /// <param name="sharedDataContainer">Container for shared data</param>
    /// <param name="mediator">Mediator for handling requests</param>
    /// <param name="sp">Service provider for dependency resolution</param>
    public IndexController(SharedDataContainer sharedDataContainer, IMediator mediator, IServiceProvider sp)
        : base(mediator, sp)
    {
        _sharedDataContainer = sharedDataContainer;
    }

    /// <summary>
    /// Gets the current value from the shared data container
    /// </summary>
    /// <returns>The current shared value</returns>
    [HttpGet]
    public IResult<string> Base()
    {
        return Result.Create(_sharedDataContainer.Value);
    }

    /// <summary>
    /// Handles demo command requests
    /// </summary>
    /// <param name="request">The demo command to process</param>
    /// <returns>Result of the command operation</returns>
    [HttpPost("command")]
    public Task<IResult> RequestOnlyAsync([FromBody] DemoCommand request)
    {
        return HandleAsync(request);
    }

    /// <summary>
    /// Handles demo query requests
    /// </summary>
    /// <param name="request">The demo query to process</param>
    /// <returns>Result containing the demo response</returns>
    [HttpGet("query")]
    public Task<IResult<DemoResponse>> RequestOnlyAsync([FromQuery] DemoQuery request)
    {
        return HandleAsync<DemoQuery, DemoResponse>(request);
    }

    /// <summary>
    /// Handles demo forbidden-command requests, always resulting in a forbidden status
    /// </summary>
    /// <param name="request">The demo forbidden command to process</param>
    /// <returns>Result of the command operation</returns>
    [HttpPost("command/forbidden")]
    public Task<IResult> RequestOnlyAsync([FromBody] DemoForbiddenCommand request)
    {
        return HandleAsync(request);
    }

    /// <summary>
    /// Handles demo conflict-command requests, always resulting in a conflict status
    /// </summary>
    /// <param name="request">The demo conflict command to process</param>
    /// <returns>Result of the command operation</returns>
    [HttpPost("command/conflict")]
    public Task<IResult> RequestOnlyAsync([FromBody] DemoConflictCommand request)
    {
        return HandleAsync(request);
    }

    /// <summary>
    /// Handles demo server-error-command requests, always resulting in an uncaught-error status
    /// </summary>
    /// <param name="request">The demo server error command to process</param>
    /// <returns>Result of the command operation</returns>
    [HttpPost("command/server-error")]
    public Task<IResult> RequestOnlyAsync([FromBody] DemoServerErrorCommand request)
    {
        return HandleAsync(request);
    }

    /// <summary>
    /// Handles demo throwing-command requests, always throwing a plain exception from the handler
    /// </summary>
    /// <param name="request">The demo throwing command to process</param>
    /// <returns>Result of the command operation</returns>
    [HttpPost("command/throw")]
    public Task<IResult> RequestOnlyAsync([FromBody] DemoThrowingCommand request)
    {
        return HandleAsync(request);
    }
}

/// <summary>
/// Handler for processing demo commands
/// </summary>
public class DemoCommandHandler : ICommandHandler<DemoCommand>
{
    /// <summary>
    /// Handles the demo command asynchronously
    /// </summary>
    /// <param name="request">The demo command to handle</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Status result indicating success or failure</returns>
    public Task<IStatusResult<OperationStatus>> HandleAsync(DemoCommand request, CancellationToken ct)
    {
        if (!request.IsOk)
            return Task.FromResult(Result.Status(OperationStatus.BadRequest).Error("Not ok"));

        return Task.FromResult(Result.Status(OperationStatus.Ok));
    }
}

/// <summary>
/// Demo command for testing command handling
/// </summary>
public class DemoCommand : ICommand
{
    /// <summary>
    /// Gets or sets a value indicating whether the command should succeed
    /// </summary>
    public bool IsOk { get; set; }
}

/// <summary>
/// Handler for processing demo commands that always result in a forbidden status
/// </summary>
public class DemoForbiddenCommandHandler : ICommandHandler<DemoForbiddenCommand>
{
    /// <summary>
    /// Handles the demo forbidden command asynchronously
    /// </summary>
    /// <param name="request">The demo forbidden command to handle</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Status result indicating a forbidden status</returns>
    public Task<IStatusResult<OperationStatus>> HandleAsync(DemoForbiddenCommand request, CancellationToken ct)
    {
        return Task.FromResult(Result.Status(OperationStatus.Forbidden).Error("Forbidden"));
    }
}

/// <summary>
/// Demo command for testing forbidden-status handling
/// </summary>
public class DemoForbiddenCommand : ICommand;

/// <summary>
/// Handler for processing demo commands that always result in a conflict status
/// </summary>
public class DemoConflictCommandHandler : ICommandHandler<DemoConflictCommand>
{
    /// <summary>
    /// Handles the demo conflict command asynchronously
    /// </summary>
    /// <param name="request">The demo conflict command to handle</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Status result indicating a conflict status</returns>
    public Task<IStatusResult<OperationStatus>> HandleAsync(DemoConflictCommand request, CancellationToken ct)
    {
        return Task.FromResult(Result.Status(OperationStatus.Conflict).Error("Conflict"));
    }
}

/// <summary>
/// Demo command for testing conflict-status handling
/// </summary>
public class DemoConflictCommand : ICommand;

/// <summary>
/// Handler for processing demo commands that always result in an uncaught-error status, which the
/// HTTP status pipe handler maps to <c>ServerException</c> (HTTP 500) via its default case
/// </summary>
public class DemoServerErrorCommandHandler : ICommandHandler<DemoServerErrorCommand>
{
    /// <summary>
    /// Handles the demo server error command asynchronously
    /// </summary>
    /// <param name="request">The demo server error command to handle</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Status result indicating an uncaught-error status</returns>
    public Task<IStatusResult<OperationStatus>> HandleAsync(DemoServerErrorCommand request, CancellationToken ct)
    {
        return Task.FromResult(Result.Status(OperationStatus.UncaughtError).Error("Server error"));
    }
}

/// <summary>
/// Demo command for testing server-error-status handling
/// </summary>
public class DemoServerErrorCommand : ICommand;

/// <summary>
/// Handler for processing demo commands that always throw a plain (non-HTTP) exception, exercising
/// the exception middleware's last-resort generic catch clause
/// </summary>
public class DemoThrowingCommandHandler : ICommandHandler<DemoThrowingCommand>
{
    /// <summary>
    /// Handles the demo throwing command asynchronously by always throwing
    /// </summary>
    /// <param name="request">The demo throwing command to handle</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Never returns; always throws</returns>
    /// <exception cref="InvalidOperationException">Always thrown to simulate an unhandled error</exception>
    public Task<IStatusResult<OperationStatus>> HandleAsync(DemoThrowingCommand request, CancellationToken ct)
    {
        throw new InvalidOperationException("boom");
    }
}

/// <summary>
/// Demo command for testing generic-exception handling
/// </summary>
public class DemoThrowingCommand : ICommand;

/// <summary>
/// Handler for processing demo queries
/// </summary>
public class DemoQueryHandler : IQueryHandler<DemoQuery, DemoResponse>
{
    /// <summary>
    /// Handles the demo query asynchronously
    /// </summary>
    /// <param name="request">The demo query to handle</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Status result containing the demo response or error</returns>
    public Task<IStatusResult<OperationStatus, DemoResponse>> HandleAsync(DemoQuery request, CancellationToken ct)
    {
        if (request.Q == 0)
            return Task.FromResult(
                Result.Status<OperationStatus, DemoResponse>(OperationStatus.NotFound, default!).Error("Not found")
            );

        return Task.FromResult(Result.Status(OperationStatus.Ok, new DemoResponse { X = request.Q }));
    }
}

/// <summary>
/// Demo query for testing query handling
/// </summary>
public class DemoQuery : IQuery
{
    /// <summary>
    /// Gets or sets the query parameter
    /// </summary>
    public int Q { get; set; }
}

/// <summary>
/// Response model for demo queries
/// </summary>
public class DemoResponse
{
    /// <summary>
    /// Gets or sets the response value
    /// </summary>
    public int X { get; set; }
}
