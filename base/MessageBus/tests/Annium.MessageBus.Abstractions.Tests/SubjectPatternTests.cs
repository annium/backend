using System;
using Annium.Testing;
using Xunit;

namespace Annium.MessageBus.Abstractions.Tests;

/// <summary>
/// Tests for <see cref="SubjectPattern"/> parsing and matching.
/// </summary>
public class SubjectPatternTests
{
    /// <summary>
    /// Well-formed patterns parse successfully.
    /// </summary>
    /// <param name="pattern">A valid pattern.</param>
    [Theory]
    [InlineData("orders.created")]
    [InlineData("orders.*.created")]
    [InlineData("orders.>")]
    [InlineData(">")]
    [InlineData("*")]
    public void Parse_Valid(string pattern)
    {
        var parsed = SubjectPattern.Parse(pattern);
        parsed.Tokens.Count.Is(pattern.Split('.').Length);
    }

    /// <summary>
    /// Malformed patterns are rejected — including <c>&gt;</c> not in the last position and empty tokens.
    /// </summary>
    /// <param name="pattern">A malformed pattern.</param>
    [Theory]
    [InlineData("")]
    [InlineData("orders.>.created")]
    [InlineData(">.created")]
    [InlineData("orders..created")]
    [InlineData("orders.cre ated")]
    public void Parse_Invalid_Throws(string pattern)
    {
        Wrap.It(() => SubjectPattern.Parse(pattern)).Throws<ArgumentException>();
    }

    /// <summary>
    /// The error for a misplaced <c>&gt;</c> identifies the violated rule.
    /// </summary>
    [Fact]
    public void Parse_MultiWildcardNotLast_MessageMentionsLast()
    {
        var ex = Wrap.It(() => SubjectPattern.Parse("orders.>.created")).Throws<ArgumentException>();
        ex.Message.Contains("last").Is(true);
    }

    /// <summary>
    /// The single-token wildcard <c>*</c> matches exactly one token in that position.
    /// </summary>
    [Fact]
    public void SingleWildcard_MatchesOneToken()
    {
        var pattern = SubjectPattern.Parse("orders.*.created");
        pattern.Matches("orders.eu.created").Is(true);
        pattern.Matches("orders.us.created").Is(true);
        pattern.Matches("orders.created").Is(false);
        pattern.Matches("orders.eu.west.created").Is(false);
    }

    /// <summary>
    /// The multi-token wildcard <c>&gt;</c> matches one or more trailing tokens (not zero).
    /// </summary>
    [Fact]
    public void MultiWildcard_MatchesTail()
    {
        var pattern = SubjectPattern.Parse("orders.>");
        pattern.HasMultiWildcard.Is(true);
        pattern.Matches("orders.created").Is(true);
        pattern.Matches("orders.eu.created").Is(true);
        pattern.Matches("orders").Is(false);
        pattern.Matches("users.created").Is(false);
    }

    /// <summary>
    /// A literal pattern matches only the exact subject.
    /// </summary>
    [Fact]
    public void Literal_MatchesExact()
    {
        var pattern = SubjectPattern.Parse("orders.created");
        pattern.Matches("orders.created").Is(true);
        pattern.Matches("orders.updated").Is(false);
        pattern.Matches("orders.created.v1").Is(false);
    }

    /// <summary>
    /// Matching against a malformed concrete subject throws.
    /// </summary>
    [Fact]
    public void Matches_InvalidSubject_Throws()
    {
        var pattern = SubjectPattern.Parse("orders.>");
        Wrap.It(() => pattern.Matches("orders..created")).Throws<ArgumentException>();
    }
}
