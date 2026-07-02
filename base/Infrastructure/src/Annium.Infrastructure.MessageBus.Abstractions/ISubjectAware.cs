namespace Annium.Infrastructure.MessageBus.Abstractions;

/// <summary>
/// Marks a message type that carries its own publish subject as a compile-time constant,
/// enabling type-based publish and subscribe without passing the subject explicitly.
/// </summary>
public interface ISubjectAware
{
    /// <summary>
    /// Gets the canonical subject this message type is published to and subscribed from.
    /// </summary>
    static abstract string Subject { get; }
}
