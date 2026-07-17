using System;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Architecture.Http.Exceptions;
using Annium.Data.Operations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Annium.AspNetCore.TestServer.Controllers;

/// <summary>
/// Additive test-only controller exercising a handler that starts writing the response body and then
/// throws, so <c>ExceptionMiddleware</c> observes an exception after <c>HttpResponse.HasStarted</c> is
/// already <c>true</c>. Kept separate from <see cref="IndexController" /> so the existing CQRS-based demo
/// endpoints (and the 11 tests pinning them) are left untouched.
/// </summary>
[Route("/")]
public class PartialWriteController : ControllerBase
{
    /// <summary>
    /// Writes a partial response body, starting the response, then throws — simulating a downstream
    /// handler that fails after streaming has already begun.
    /// </summary>
    /// <returns>Never completes normally; always throws after writing.</returns>
    /// <exception cref="InvalidOperationException">Always thrown after the partial write.</exception>
    [HttpGet("partial-write")]
    public async Task GetAsync()
    {
        await Response.WriteAsync("partial");
        throw new InvalidOperationException("boom after start");
    }

    /// <summary>
    /// Writes a partial response body, starting the response, then throws a <see cref="NotFoundException" />
    /// — simulating a downstream handler in one of <c>ExceptionMiddleware</c>'s typed catch clauses failing
    /// after streaming has already begun.
    /// </summary>
    /// <returns>Never completes normally; always throws after writing.</returns>
    /// <exception cref="NotFoundException">Always thrown after the partial write.</exception>
    [HttpGet("partial-write-not-found")]
    public async Task GetNotFoundAsync()
    {
        await Response.WriteAsync("partial");
        throw new NotFoundException(Result.Status(OperationStatus.NotFound).Error("not found after start"));
    }

    /// <summary>
    /// Writes a partial response body, starting the response, then throws a <see cref="ValidationException" />
    /// — simulating a downstream handler in one of <c>ExceptionMiddleware</c>'s typed catch clauses failing
    /// after streaming has already begun.
    /// </summary>
    /// <returns>Never completes normally; always throws after writing.</returns>
    /// <exception cref="ValidationException">Always thrown after the partial write.</exception>
    [HttpGet("partial-write-validation")]
    public async Task GetValidationAsync()
    {
        await Response.WriteAsync("partial");
        throw new ValidationException(Result.Status(OperationStatus.BadRequest).Error("validation after start"));
    }

    /// <summary>
    /// Writes a partial response body, starting the response, then throws a <see cref="ForbiddenException" />
    /// — simulating a downstream handler in one of <c>ExceptionMiddleware</c>'s typed catch clauses failing
    /// after streaming has already begun.
    /// </summary>
    /// <returns>Never completes normally; always throws after writing.</returns>
    /// <exception cref="ForbiddenException">Always thrown after the partial write.</exception>
    [HttpGet("partial-write-forbidden")]
    public async Task GetForbiddenAsync()
    {
        await Response.WriteAsync("partial");
        throw new ForbiddenException(Result.Status(OperationStatus.Forbidden).Error("forbidden after start"));
    }

    /// <summary>
    /// Writes a partial response body, starting the response, then throws a <see cref="ConflictException" />
    /// — simulating a downstream handler in one of <c>ExceptionMiddleware</c>'s typed catch clauses failing
    /// after streaming has already begun.
    /// </summary>
    /// <returns>Never completes normally; always throws after writing.</returns>
    /// <exception cref="ConflictException">Always thrown after the partial write.</exception>
    [HttpGet("partial-write-conflict")]
    public async Task GetConflictAsync()
    {
        await Response.WriteAsync("partial");
        throw new ConflictException(Result.Status(OperationStatus.Conflict).Error("conflict after start"));
    }

    /// <summary>
    /// Writes a partial response body, starting the response, then throws a <see cref="ServerException" />
    /// — simulating a downstream handler in the structurally distinct <c>ServerException</c> catch clause
    /// (which logs unconditionally before its own <c>HasStarted</c> early-return) failing after streaming
    /// has already begun.
    /// </summary>
    /// <returns>Never completes normally; always throws after writing.</returns>
    /// <exception cref="ServerException">Always thrown after the partial write.</exception>
    [HttpGet("partial-write-server-error")]
    public async Task GetServerErrorAsync()
    {
        await Response.WriteAsync("partial");
        throw new ServerException(Result.Status(OperationStatus.UncaughtError).Error("server error after start"));
    }
}
