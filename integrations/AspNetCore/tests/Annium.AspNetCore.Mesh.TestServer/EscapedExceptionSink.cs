using System;

namespace Annium.AspNetCore.Mesh.TestServer;

/// <summary>
/// Test-only sink that records any exception escaping the middleware chain wrapped by this test host's
/// <c>Program</c>, instead of letting the wrapper silently swallow it. Complements
/// <see cref="RequestCompletionSignal" />: while that signal proves the request pipeline settled, this sink
/// makes visible whether a secondary exception escaped downstream middleware (in particular
/// <c>Annium.AspNetCore.Mesh.Internal.Middleware.WebSocketsMiddleware.InvokeAsync</c>) while handling that
/// request. Hosts that need this observability register an instance of this class in their DI container;
/// hosts that don't care about it simply leave it unregistered, in which case the wrapping middleware still
/// swallows silently, as before.
/// </summary>
public sealed class EscapedExceptionSink
{
    /// <summary>
    /// The exception recorded by <see cref="Record" />, or <c>null</c> if nothing has escaped.
    /// </summary>
    public Exception? Escaped { get; private set; }

    /// <summary>
    /// Records an exception that escaped the wrapped downstream middleware chain.
    /// </summary>
    /// <param name="exception">The exception that escaped.</param>
    public void Record(Exception exception) => Escaped = exception;
}
