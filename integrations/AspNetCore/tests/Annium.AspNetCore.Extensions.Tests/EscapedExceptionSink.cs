using System;

namespace Annium.AspNetCore.Extensions.Tests;

/// <summary>
/// Test-only sink that records any exception escaping the middleware chain wrapped by
/// <see cref="EscapedExceptionStartupFilter" />, instead of letting it propagate to the test-hosting
/// transport (where it would otherwise surface as an aborted connection rather than an observable value).
/// Mirrors the equivalent sink already used by <c>Annium.AspNetCore.Mesh.TestServer</c> for the analogous
/// WebSocket regression coverage.
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
