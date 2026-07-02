using Annium.Testing;
using Xunit;

namespace Annium.Infrastructure.MessageBus.Abstractions.Tests;

/// <summary>
/// Tests that a subject-aware type's subject is resolved from its static abstract member.
/// </summary>
public class SubjectAwareResolutionTests
{
    /// <summary>
    /// <see cref="Subject.Of{T}"/> returns the static subject declared by the type.
    /// </summary>
    [Fact]
    public void Of_ResolvesStaticSubject()
    {
        Subject.Of<TestMessage>().Is("test.message");
    }

    /// <summary>
    /// A subject-aware test message declaring its subject as a compile-time constant.
    /// </summary>
    private sealed record TestMessage : ISubjectAware
    {
        /// <summary>
        /// Gets the subject for this message type.
        /// </summary>
        public static string Subject => "test.message";
    }
}
