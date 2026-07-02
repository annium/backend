using System;
using Annium.Testing;
using Xunit;

namespace Annium.Infrastructure.MessageBus.Abstractions.Tests;

/// <summary>
/// Tests for <see cref="Subject"/> concrete-subject validation.
/// </summary>
public class SubjectValidationTests
{
    /// <summary>
    /// Valid subjects pass validation and are reported as valid.
    /// </summary>
    /// <param name="subject">A well-formed subject.</param>
    [Theory]
    [InlineData("orders")]
    [InlineData("orders.created")]
    [InlineData("orders.eu.created")]
    [InlineData("order-events.v1.created_at")]
    public void Valid_Passes(string subject)
    {
        Subject.Validate(subject);
        Subject.IsValid(subject).Is(true);
    }

    /// <summary>
    /// Malformed subjects are rejected by both <see cref="Subject.Validate"/> and <see cref="Subject.IsValid"/>.
    /// </summary>
    /// <param name="subject">A malformed subject.</param>
    [Theory]
    [InlineData("")]
    [InlineData(".orders")]
    [InlineData("orders.")]
    [InlineData("orders..created")]
    [InlineData("orders create")]
    [InlineData("orders.*")]
    [InlineData("orders.>")]
    [InlineData("orders/created")]
    public void Invalid_Throws(string subject)
    {
        Wrap.It(() => Subject.Validate(subject)).Throws<ArgumentException>();
        Subject.IsValid(subject).Is(false);
    }

    /// <summary>
    /// A null subject is rejected.
    /// </summary>
    [Fact]
    public void Null_Throws()
    {
        Wrap.It(() => Subject.Validate(null!)).Throws<ArgumentException>();
        Subject.IsValid(null!).Is(false);
    }
}
