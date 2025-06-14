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
        return Result.New(_sharedDataContainer.Value);
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
