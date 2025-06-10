using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Architecture.CQRS.Commands;
using Annium.Architecture.CQRS.Queries;
using Annium.AspNetCore.Extensions.Controllers;
using Annium.AspNetCore.TestServer.Components;
using Annium.Core.Mediator;
using Annium.Data.Operations;
using Microsoft.AspNetCore.Mvc;

namespace Annium.AspNetCore.TestServer.Controllers;

[Route("/")]
public class IndexController : ServerController
{
    private readonly SharedDataContainer _sharedDataContainer;

    public IndexController(SharedDataContainer sharedDataContainer, IMediator mediator, IServiceProvider sp)
        : base(mediator, sp)
    {
        _sharedDataContainer = sharedDataContainer;
    }

    [HttpGet]
    public IResult<string> Base()
    {
        return Result.New(_sharedDataContainer.Value);
    }

    [HttpPost("command")]
    public Task<IResult> RequestOnlyAsync([FromBody] DemoCommand request)
    {
        return HandleAsync(request);
    }

    [HttpGet("query")]
    public Task<IResult<DemoResponse>> RequestOnlyAsync([FromQuery] DemoQuery request)
    {
        return HandleAsync<DemoQuery, DemoResponse>(request);
    }
}

public class DemoCommandHandler : ICommandHandler<DemoCommand>
{
    public Task<IStatusResult<OperationStatus>> HandleAsync(DemoCommand request, CancellationToken ct)
    {
        if (!request.IsOk)
            return Task.FromResult(Result.Status(OperationStatus.BadRequest).Error("Not ok"));

        return Task.FromResult(Result.Status(OperationStatus.Ok));
    }
}

public class DemoCommand : ICommand
{
    public bool IsOk { get; set; }
}

public class DemoQueryHandler : IQueryHandler<DemoQuery, DemoResponse>
{
    public Task<IStatusResult<OperationStatus, DemoResponse>> HandleAsync(DemoQuery request, CancellationToken ct)
    {
        if (request.Q == 0)
            return Task.FromResult(
                Result.Status<OperationStatus, DemoResponse>(OperationStatus.NotFound, default!).Error("Not found")
            );

        return Task.FromResult(Result.Status(OperationStatus.Ok, new DemoResponse { X = request.Q }));
    }
}

public class DemoQuery : IQuery
{
    public int Q { get; set; }
}

public class DemoResponse
{
    public int X { get; set; }
}
