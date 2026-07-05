using System.Collections.Generic;
using System.Threading.Tasks;

namespace Annium.MessageBus.Abstractions.Tests;

/// <summary>
/// The inbound message handed to the pipeline; forwards transport-level completion to the owning transport.
/// </summary>
public sealed class FakeIncomingMessage : ITransportIncomingMessage
{
    /// <summary>
    /// The owning transport.
    /// </summary>
    private readonly FakeTransport _transport;

    /// <summary>
    /// Initializes a new instance of the <see cref="FakeIncomingMessage"/> class.
    /// </summary>
    /// <param name="transport">The owning transport.</param>
    /// <param name="subject">The subject.</param>
    /// <param name="body">The serialized body.</param>
    /// <param name="headers">The headers.</param>
    public FakeIncomingMessage(
        FakeTransport transport,
        string subject,
        string body,
        IReadOnlyDictionary<string, string> headers
    )
    {
        _transport = transport;
        Subject = subject;
        Body = body;
        Headers = headers;
    }

    /// <inheritdoc />
    public string Subject { get; }

    /// <inheritdoc />
    public string Body { get; }

    /// <inheritdoc />
    public IReadOnlyDictionary<string, string> Headers { get; }

    /// <inheritdoc />
    public Task CompleteAsync()
    {
        _transport.OnCompleted();
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task AbandonAsync()
    {
        _transport.OnAbandoned();
        return Task.CompletedTask;
    }
}
