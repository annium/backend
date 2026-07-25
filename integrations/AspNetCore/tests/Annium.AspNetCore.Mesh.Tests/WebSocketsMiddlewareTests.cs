using System;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Annium.Testing;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Annium.AspNetCore.Mesh.Tests;

/// <summary>
/// Pins the four branches of <c>Annium.AspNetCore.Mesh.Internal.Middleware.WebSocketsMiddleware.InvokeAsync</c>
/// (path mismatch, non-WebSocket request, happy path, catch-all failure) plus the constructor's
/// <c>applicationLifetime.ApplicationStopping.Register(_coordinator.Dispose)</c> side effect, using
/// recording/throwing <c>IServerConnectionFactory&lt;WebSocket&gt;</c> / <c>ICoordinator</c> doubles wired
/// through dedicated <see cref="Annium.AspNetCore.IntegrationTesting.TestHostBase{TEntryPoint}" /> subclasses.
/// </summary>
public class WebSocketsMiddlewareTests : TestBase
{
    /// <summary>
    /// Initializes a new instance of the WebSocketsMiddlewareTests class
    /// </summary>
    /// <param name="outputHelper">The test output helper for logging</param>
    public WebSocketsMiddlewareTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Tests that a request whose path does not start with the configured <c>PathMatch</c> segment is passed
    /// through to the next middleware untouched, rather than being handled (or swallowed) by the mesh middleware.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task PathMismatch_PassesThroughToNextMiddleware()
    {
        // arrange
        var host = new RecordingTestHost(OutputHelper);
        await using var testHost = await host.StartAsync();
        using var client = testHost.Server.CreateClient();

        // act
        var response = await client.GetAsync("/not-mesh", TestContext.Current.CancellationToken);

        // assert
        response.StatusCode.Is(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        body.Is("passed-through");
    }

    /// <summary>
    /// Tests that a plain HTTP request to the matched path (not a WebSocket upgrade) is rejected with
    /// 400 Bad Request and the exact serialized error body, without ever touching the connection factory
    /// or coordinator.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task NonWebSocketRequest_ReturnsBadRequestWithNotAWebSocketConnectionError()
    {
        // arrange
        var host = new RecordingTestHost(OutputHelper);
        await using var testHost = await host.StartAsync();
        using var client = testHost.Server.CreateClient();

        // act
        var response = await client.GetAsync("/mesh", TestContext.Current.CancellationToken);

        // assert
        response.StatusCode.Is(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        body.Is("""{"plainErrors":["Not a WebSocket connection"],"labeledErrors":{}}""");
    }

    /// <summary>
    /// Tests that a real WebSocket upgrade request to the matched path is accepted, the resulting
    /// <see cref="WebSocket" /> is handed to the connection factory in the <see cref="WebSocketState.Open" />
    /// state, and the exact connection instance the factory returns is subsequently handed off to the
    /// coordinator — pinning the accept → create → handle ordering end-to-end through a real handshake.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task WebSocketRequest_AcceptsCreatesConnectionAndHandsOffToCoordinator()
    {
        // arrange
        var host = new RecordingTestHost(OutputHelper);
        await using var testHost = await host.StartAsync();
        var wsClient = testHost.Server.CreateWebSocketClient();

        // act
        using var clientSocket = await wsClient.ConnectAsync(
            new Uri("ws://localhost/mesh"),
            TestContext.Current.CancellationToken
        );

        // assert
        var acceptedSocket = await host.ConnectionFactory.Created.WaitAsync(
            TimeSpan.FromSeconds(5),
            TestContext.Current.CancellationToken
        );
        acceptedSocket.State.Is(WebSocketState.Open);

        var handledConnection = await host.Coordinator.Handled.WaitAsync(
            TimeSpan.FromSeconds(5),
            TestContext.Current.CancellationToken
        );
        handledConnection.Is(host.ConnectionFactory.Connection);

        // let HandleAsync return so the middleware can complete the request
        host.Coordinator.Release();
    }

    /// <summary>
    /// Tests that the ASP.NET Core hosting layer disposes the mesh middleware's coordinator when the
    /// application starts shutting down, pinning the
    /// <c>applicationLifetime.ApplicationStopping.Register(_coordinator.Dispose)</c> wiring registered in the
    /// middleware constructor.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task ApplicationStopping_DisposesCoordinator()
    {
        // arrange
        var host = new RecordingTestHost(OutputHelper);
        await using var testHost = await host.StartAsync();

        // the middleware is a lazily-activated DI singleton: its constructor (and the ApplicationStopping
        // registration inside it) only runs once a request actually reaches it, so route one through first.
        using (var client = testHost.Server.CreateClient())
            await client.GetAsync("/not-mesh", TestContext.Current.CancellationToken);

        var lifetime = testHost.Get<IHostApplicationLifetime>();

        // act
        lifetime.StopApplication();

        // assert
        await host.Coordinator.DisposedSignal.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        host.Coordinator.Disposed.IsTrue();
    }

    /// <summary>
    /// Tests that a connection-factory failure is caught internally without corrupting the WebSocket
    /// handshake already sent to the client, and — critically — that the coordinator is never invoked when
    /// the factory fails first, so a broken/absent connection can never reach connection handling.
    /// </summary>
    /// <remarks>
    /// The literal "500 Bad Request + serialized error body" described for the catch-all branch is not
    /// observable here: by the time the connection factory or coordinator run, <c>AcceptWebSocketAsync</c>
    /// has already completed the WebSocket upgrade handshake, so <see cref="Microsoft.AspNetCore.Http.HttpContext.Response" />
    /// has already started and no HTTP status/body can be written for it anymore. After the fix, the
    /// middleware detects exactly this via <c>HttpResponse.HasStarted</c> and, instead of attempting (and
    /// failing) to write a response, logs the failure and attempts a graceful WebSocket close — see
    /// <see cref="CoordinatorThrowsAfterUpgrade_NoSecondaryExceptionEscapesMiddleware" /> for the dedicated
    /// regression test pinning that no secondary exception escapes <c>InvokeAsync</c> for this class of
    /// post-upgrade failure.
    /// </remarks>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task ConnectionFactoryThrows_HandshakeCompletesButCoordinatorIsNeverInvoked()
    {
        // arrange
        var host = new ThrowingFactoryTestHost(OutputHelper);
        await using var testHost = await host.StartAsync();
        var wsClient = testHost.Server.CreateWebSocketClient();

        // act
        using var clientSocket = await wsClient.ConnectAsync(
            new Uri("ws://localhost/mesh"),
            TestContext.Current.CancellationToken
        );

        // assert — the handshake itself is unaffected by the later factory failure
        clientSocket.State.Is(WebSocketState.Open);

        // confirm the scenario is actually exercised (not a vacuous pass): the connection factory really was
        // invoked, and — being ThrowingConnectionFactory — necessarily threw immediately afterwards.
        await host.ConnectionFactory.Invoked.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        // the fixed middleware attempts a graceful close of the accepted socket; receive the resulting close
        // frame and echo it back — a real client would do the same — so the close handshake can complete on
        // the server side. Receiving it here also positively confirms the graceful close actually happened,
        // rather than the socket being left dangling.
        var receiveBuffer = new byte[16];
        var closeReceive = await clientSocket
            .ReceiveAsync(receiveBuffer, TestContext.Current.CancellationToken)
            .WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        closeReceive.MessageType.Is(WebSocketMessageType.Close);
        await clientSocket.CloseOutputAsync(
            WebSocketCloseStatus.NormalClosure,
            null,
            TestContext.Current.CancellationToken
        );

        // wait for a positive, structural signal that WebSocketsMiddleware.InvokeAsync has fully returned for
        // this request: the test host's Program wraps the middleware chain and only fires RequestCompleted
        // after awaiting it. Because the coordinator can only ever be invoked from inside that same call,
        // observing this signal is a genuine happens-after relationship with everything InvokeAsync did (or
        // didn't do) while handling the request — including any call to HandleAsync — so checking
        // Coordinator.Handled afterwards proves "never invoked" as a settled fact rather than racing a fixed
        // wait window against it.
        await host.RequestCompleted.Completed.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        host.Coordinator.Handled.IsCompleted.IsFalse();

        // the fixed middleware must not let a secondary exception escape while handling this post-upgrade
        // failure either
        host.EscapedException.Escaped.IsNull();
    }

    /// <summary>
    /// Tests that a coordinator failure is reached only after a real connection has been created and handed
    /// off — i.e. the failure happens downstream of a successful accept + create, not in place of it.
    /// </summary>
    /// <remarks>
    /// As with <see cref="ConnectionFactoryThrows_HandshakeCompletesButCoordinatorIsNeverInvoked" />, the
    /// "500 + error body" contract is not observable here for the same reason: the WebSocket handshake has
    /// already completed by the time the coordinator runs, so the response has already started and no HTTP
    /// status/body can be written for it anymore. See
    /// <see cref="CoordinatorThrowsAfterUpgrade_NoSecondaryExceptionEscapesMiddleware" /> for the dedicated
    /// regression test pinning that the fixed middleware logs the failure and attempts a graceful WebSocket
    /// close instead, without letting a secondary exception escape <c>InvokeAsync</c>. This test does not
    /// assert the accepted socket's <see cref="WebSocketState" />: as soon as the coordinator throws, the fixed
    /// middleware immediately starts a server-initiated graceful close on that same socket (see
    /// <see cref="CoordinatorThrowsAfterUpgrade_NoSecondaryExceptionEscapesMiddleware" />), so its state races
    /// between <see cref="WebSocketState.Open" /> and <see cref="WebSocketState.CloseSent" /> depending on
    /// scheduling — the "handed off before failing" ordering this test pins is instead proven deterministically
    /// via <see cref="TestDoubles.ThrowingCoordinator.Invoked" /> receiving the exact connection instance the factory
    /// created, which the coordinator can only have received after <c>CreateAsync</c> produced it.
    /// </remarks>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task CoordinatorThrows_HandshakeCompletesAndConnectionIsHandedOffBeforeFailing()
    {
        // arrange
        var host = new ThrowingCoordinatorTestHost(OutputHelper);
        await using var testHost = await host.StartAsync();
        var wsClient = testHost.Server.CreateWebSocketClient();

        // act
        using var clientSocket = await wsClient.ConnectAsync(
            new Uri("ws://localhost/mesh"),
            TestContext.Current.CancellationToken
        );

        // assert — the handshake itself completed successfully
        clientSocket.State.Is(WebSocketState.Open);

        // the connection factory really did create a connection from the accepted socket ...
        await host.ConnectionFactory.Created.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        // ... and the coordinator was invoked (and threw) with that exact same connection instance, proving the
        // hand-off happened before the failure rather than the coordinator running in place of it
        var handledConnection = await host.Coordinator.Invoked.WaitAsync(
            TimeSpan.FromSeconds(5),
            TestContext.Current.CancellationToken
        );
        handledConnection.Is(host.ConnectionFactory.Connection);
    }

    /// <summary>
    /// Pins the fix for a real bug: when the coordinator fails after the WebSocket upgrade has already
    /// completed, <c>WebSocketsMiddleware.InvokeAsync</c> must not let a secondary exception escape by trying
    /// to write an HTTP response to a request whose response has already started (which throws
    /// <see cref="InvalidOperationException" />, masking the original, already-logged failure). Instead, the
    /// middleware must detect this via <c>HttpResponse.HasStarted</c> and attempt a graceful WebSocket close.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task CoordinatorThrowsAfterUpgrade_NoSecondaryExceptionEscapesMiddleware()
    {
        // arrange
        var host = new ThrowingCoordinatorTestHost(OutputHelper);
        await using var testHost = await host.StartAsync();
        var wsClient = testHost.Server.CreateWebSocketClient();

        // act
        using var clientSocket = await wsClient.ConnectAsync(
            new Uri("ws://localhost/mesh"),
            TestContext.Current.CancellationToken
        );

        // confirm the scenario is actually exercised (not a vacuous pass): the coordinator really was invoked,
        // and — being ThrowingCoordinator — necessarily threw immediately afterwards.
        await host.Coordinator.Invoked.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        // the fixed middleware attempts a graceful close of the accepted socket; receive the resulting close
        // frame and echo it back — a real client would do the same — so the close handshake can complete on
        // the server side. Receiving it here also positively confirms the graceful close actually happened,
        // rather than the socket being left dangling.
        var receiveBuffer = new byte[16];
        var closeReceive = await clientSocket
            .ReceiveAsync(receiveBuffer, TestContext.Current.CancellationToken)
            .WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        closeReceive.MessageType.Is(WebSocketMessageType.Close);
        await clientSocket.CloseOutputAsync(
            WebSocketCloseStatus.NormalClosure,
            null,
            TestContext.Current.CancellationToken
        );

        // wait for a positive, structural signal that WebSocketsMiddleware.InvokeAsync has fully returned for
        // this request: the test host's Program wraps the middleware chain and only fires RequestCompleted
        // after awaiting it (recording any escaped exception on EscapedException first) — so observing this
        // signal is a genuine happens-after relationship with everything InvokeAsync did while handling the
        // request, rather than racing a fixed wait window against it.
        await host.RequestCompleted.Completed.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        // assert — no secondary exception escaped WebSocketsMiddleware.InvokeAsync
        host.EscapedException.Escaped.IsNull();
    }
}
