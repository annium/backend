using System;
using System.Threading.Tasks;
using Annium.Architecture.Http.Exceptions;
using Annium.Testing;
using Xunit;

namespace Annium.AspNetCore.Extensions.Tests;

/// <summary>
/// Pins the fix for a real bug: when a downstream handler has already started writing the response body
/// before throwing, <c>ExceptionMiddleware.InvokeAsync</c> must not attempt to set
/// <c>HttpResponse.StatusCode</c> (which throws a secondary <see cref="InvalidOperationException" />,
/// "the response has already started", masking the original failure). Instead, the middleware must detect
/// this via <c>HttpResponse.HasStarted</c>, log the original exception, and return without attempting to
/// write an HTTP status/body for it.
/// </summary>
public class ExceptionMiddlewareTests : TestBase
{
    /// <summary>
    /// Initializes a new instance of the ExceptionMiddlewareTests class
    /// </summary>
    /// <param name="outputHelper">The test output helper for logging</param>
    public ExceptionMiddlewareTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Tests that a handler which starts writing the response body and then throws does not cause a
    /// secondary exception to escape the middleware chain, and that the original exception is still logged
    /// even though no HTTP status/body can be written for it anymore.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task ResponseStartedBeforeThrow_NoSecondaryExceptionEscapesAndOriginalIsLogged()
    {
        // arrange
        var host = new PartialWriteTestHost(OutputHelper);
        await using var testHost = await host.StartAsync();
        using var client = testHost.Server.CreateClient();

        // act
        using var response = await client.GetAsync("/partial-write", TestContext.Current.CancellationToken);

        // assert — no secondary exception escaped the wrapped middleware chain
        host.EscapedException.Escaped.IsNull();

        // assert — the original exception was still logged, so the cause isn't lost
        var loggedError = host.RecordingLogger.LoggedError.IsNotNull();
        loggedError.As<InvalidOperationException>().Message.Is("boom after start");
    }

    /// <summary>
    /// Tests that a handler which starts writing the response body and then throws a
    /// <see cref="NotFoundException" /> — one of the four typed exceptions handled by dedicated catch
    /// clauses in <c>ExceptionMiddleware.InvokeAsync</c> — does not cause a secondary exception to escape
    /// the middleware chain, and that the original typed exception is still logged even though no HTTP
    /// status/body can be written for it anymore. Pins the <c>WriteFailedSilently</c> guard reached from the
    /// <see cref="NotFoundException" /> catch clause specifically, which the generic-exception test above
    /// does not exercise.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task ResponseStartedBeforeThrow_NotFoundException_NoSecondaryExceptionEscapesAndOriginalIsLogged()
    {
        // arrange
        var host = new PartialWriteTestHost(OutputHelper);
        await using var testHost = await host.StartAsync();
        using var client = testHost.Server.CreateClient();

        // act
        using var response = await client.GetAsync("/partial-write-not-found", TestContext.Current.CancellationToken);

        // assert — no secondary exception escaped the wrapped middleware chain
        host.EscapedException.Escaped.IsNull();

        // assert — the original typed exception was still logged, so the cause isn't lost
        var loggedError = host.RecordingLogger.LoggedError.IsNotNull();
        loggedError.As<NotFoundException>();
    }

    /// <summary>
    /// Tests that a handler which starts writing the response body and then throws a
    /// <see cref="ValidationException" /> — another of the four typed exceptions handled by dedicated catch
    /// clauses in <c>ExceptionMiddleware.InvokeAsync</c> — does not cause a secondary exception to escape
    /// the middleware chain, and that the original typed exception is still logged even though no HTTP
    /// status/body can be written for it anymore. Pins the <c>WriteFailedSilently</c> guard reached from the
    /// <see cref="ValidationException" /> catch clause specifically.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task ResponseStartedBeforeThrow_ValidationException_NoSecondaryExceptionEscapesAndOriginalIsLogged()
    {
        // arrange
        var host = new PartialWriteTestHost(OutputHelper);
        await using var testHost = await host.StartAsync();
        using var client = testHost.Server.CreateClient();

        // act
        using var response = await client.GetAsync("/partial-write-validation", TestContext.Current.CancellationToken);

        // assert — no secondary exception escaped the wrapped middleware chain
        host.EscapedException.Escaped.IsNull();

        // assert — the original typed exception was still logged, so the cause isn't lost
        var loggedError = host.RecordingLogger.LoggedError.IsNotNull();
        loggedError.As<ValidationException>();
    }

    /// <summary>
    /// Tests that a handler which starts writing the response body and then throws a
    /// <see cref="ForbiddenException" /> — another of the four typed exceptions handled by dedicated catch
    /// clauses in <c>ExceptionMiddleware.InvokeAsync</c> — does not cause a secondary exception to escape
    /// the middleware chain, and that the original typed exception is still logged even though no HTTP
    /// status/body can be written for it anymore. Pins the <c>WriteFailedSilently</c> guard reached from the
    /// <see cref="ForbiddenException" /> catch clause specifically.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task ResponseStartedBeforeThrow_ForbiddenException_NoSecondaryExceptionEscapesAndOriginalIsLogged()
    {
        // arrange
        var host = new PartialWriteTestHost(OutputHelper);
        await using var testHost = await host.StartAsync();
        using var client = testHost.Server.CreateClient();

        // act
        using var response = await client.GetAsync("/partial-write-forbidden", TestContext.Current.CancellationToken);

        // assert — no secondary exception escaped the wrapped middleware chain
        host.EscapedException.Escaped.IsNull();

        // assert — the original typed exception was still logged, so the cause isn't lost
        var loggedError = host.RecordingLogger.LoggedError.IsNotNull();
        loggedError.As<ForbiddenException>();
    }

    /// <summary>
    /// Tests that a handler which starts writing the response body and then throws a
    /// <see cref="ConflictException" /> — another of the four typed exceptions handled by dedicated catch
    /// clauses in <c>ExceptionMiddleware.InvokeAsync</c> — does not cause a secondary exception to escape
    /// the middleware chain, and that the original typed exception is still logged even though no HTTP
    /// status/body can be written for it anymore. Pins the <c>WriteFailedSilently</c> guard reached from the
    /// <see cref="ConflictException" /> catch clause specifically.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task ResponseStartedBeforeThrow_ConflictException_NoSecondaryExceptionEscapesAndOriginalIsLogged()
    {
        // arrange
        var host = new PartialWriteTestHost(OutputHelper);
        await using var testHost = await host.StartAsync();
        using var client = testHost.Server.CreateClient();

        // act
        using var response = await client.GetAsync("/partial-write-conflict", TestContext.Current.CancellationToken);

        // assert — no secondary exception escaped the wrapped middleware chain
        host.EscapedException.Escaped.IsNull();

        // assert — the original typed exception was still logged, so the cause isn't lost
        var loggedError = host.RecordingLogger.LoggedError.IsNotNull();
        loggedError.As<ConflictException>();
    }

    /// <summary>
    /// Tests that a handler which starts writing the response body and then throws a
    /// <see cref="ServerException" /> — the structurally distinct fifth typed exception in
    /// <c>ExceptionMiddleware.InvokeAsync</c>, which logs the exception unconditionally via <c>this.Error(e)</c>
    /// before its own dedicated <c>if (context.Response.HasStarted) return;</c> early-return, rather than
    /// going through <c>WriteFailedSilently</c> — does not cause a secondary exception to escape the
    /// middleware chain. The original exception is logged unconditionally by this catch clause regardless of
    /// whether the response has started, so the key assertion pinning the started-path specifically is that
    /// no secondary exception escapes; the logged-exception assertion additionally confirms the cause is
    /// preserved.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task ResponseStartedBeforeThrow_ServerException_NoSecondaryExceptionEscapesAndOriginalIsLogged()
    {
        // arrange
        var host = new PartialWriteTestHost(OutputHelper);
        await using var testHost = await host.StartAsync();
        using var client = testHost.Server.CreateClient();

        // act
        using var response = await client.GetAsync(
            "/partial-write-server-error",
            TestContext.Current.CancellationToken
        );

        // assert — no secondary exception escaped the wrapped middleware chain
        host.EscapedException.Escaped.IsNull();

        // assert — the original exception was still logged (this catch clause logs unconditionally)
        var loggedError = host.RecordingLogger.LoggedError.IsNotNull();
        loggedError.As<ServerException>();
    }
}
