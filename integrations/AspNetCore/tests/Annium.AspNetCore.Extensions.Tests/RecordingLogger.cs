using System;
using System.Collections.Generic;
using Annium.Logging;

namespace Annium.AspNetCore.Extensions.Tests;

/// <summary>
/// Test-only <see cref="ILogger" /> double that records the exception passed to <see cref="Error" />,
/// so tests can pin that a given failure was actually logged (rather than silently lost) even when the
/// response has already started and no HTTP error body can be written for it. Registered as a singleton via
/// <see cref="Microsoft.Extensions.Hosting.IHostBuilder.ConfigureServices" />, overriding the scoped
/// <c>ILogger</c> that <c>Annium.Logging.Shared</c>'s service registration installs for the hosted
/// application — the last registration for a service type wins when resolved, the same override technique
/// already used by <c>Annium.AspNetCore.Mesh.Tests</c>' recording connection-factory/coordinator doubles.
/// </summary>
internal sealed class RecordingLogger : ILogger
{
    /// <summary>
    /// The exception passed to the most recent <see cref="Error" /> call, or <c>null</c> if none occurred.
    /// </summary>
    public Exception? LoggedError { get; private set; }

    /// <summary>
    /// No-op: this suite only pins that failures reach <see cref="Error" />, so non-error log calls are ignored.
    /// </summary>
    /// <param name="subject">The subject object being logged.</param>
    /// <param name="file">The source file where the log was generated.</param>
    /// <param name="member">The member (method/property) where the log was generated.</param>
    /// <param name="line">The line number where the log was generated.</param>
    /// <param name="level">The log level of the message.</param>
    /// <param name="message">The message to be logged.</param>
    /// <param name="data">Additional data associated with the log entry.</param>
    public void Log(
        object subject,
        string file,
        string member,
        int line,
        LogLevel level,
        string message,
        IReadOnlyList<object?> data
    )
    {
        // not exercised by this suite: only Error calls are pinned
    }

    /// <summary>
    /// Records <paramref name="ex" /> as <see cref="LoggedError" /> so the test can assert it was logged.
    /// </summary>
    /// <param name="subject">The subject object being logged.</param>
    /// <param name="file">The source file where the error occurred.</param>
    /// <param name="member">The member (method/property) where the error occurred.</param>
    /// <param name="line">The line number where the error occurred.</param>
    /// <param name="ex">The exception that was thrown.</param>
    /// <param name="data">Additional data associated with the log entry.</param>
    public void Error(object subject, string file, string member, int line, Exception ex, IReadOnlyList<object?> data)
    {
        LoggedError = ex;
    }
}
