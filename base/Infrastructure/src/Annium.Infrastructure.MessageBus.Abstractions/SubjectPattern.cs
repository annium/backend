using System;
using System.Collections.Generic;

namespace Annium.Infrastructure.MessageBus.Abstractions;

/// <summary>
/// A parsed subscription pattern in the canonical wildcard syntax: <c>*</c> matches exactly one token,
/// <c>&gt;</c> matches one or more trailing tokens and may only appear as the last token. Adapters translate
/// <see cref="Tokens"/> into their broker-native form (Kafka regex, RabbitMQ <c>*</c>/<c>#</c>, NATS native).
/// </summary>
public sealed class SubjectPattern
{
    private const string SingleWildcard = "*";
    private const string MultiWildcard = ">";

    private SubjectPattern(IReadOnlyList<string> tokens, bool hasMultiWildcard)
    {
        Tokens = tokens;
        HasMultiWildcard = hasMultiWildcard;
    }

    /// <summary>
    /// Gets the pattern tokens (literals, <c>*</c>, or a trailing <c>&gt;</c>).
    /// </summary>
    public IReadOnlyList<string> Tokens { get; }

    /// <summary>
    /// Gets a value indicating whether the pattern ends with the multi-token wildcard <c>&gt;</c>.
    /// </summary>
    public bool HasMultiWildcard { get; }

    /// <summary>
    /// Parses a subscription pattern, throwing if it is malformed.
    /// </summary>
    /// <param name="pattern">The pattern to parse.</param>
    /// <returns>The parsed pattern.</returns>
    /// <exception cref="ArgumentException">Thrown when the pattern is null, empty, or malformed.</exception>
    public static SubjectPattern Parse(string pattern)
    {
        if (string.IsNullOrEmpty(pattern))
            throw new ArgumentException("Pattern must be a non-empty string.", nameof(pattern));

        var tokens = pattern.Split('.');
        var hasMultiWildcard = false;

        for (var i = 0; i < tokens.Length; i++)
        {
            var token = tokens[i];

            if (token == MultiWildcard)
            {
                if (i != tokens.Length - 1)
                    throw new ArgumentException(
                        $"Pattern '{pattern}': '>' may only appear as the last token.",
                        nameof(pattern)
                    );
                hasMultiWildcard = true;
                continue;
            }

            if (token == SingleWildcard)
                continue;

            if (token.Length == 0)
                throw new ArgumentException(
                    $"Pattern '{pattern}' has an empty token (no leading/trailing/double dots).",
                    nameof(pattern)
                );

            for (var j = 0; j < token.Length; j++)
                if (!Subject.IsValidTokenChar(token[j]))
                    throw new ArgumentException(
                        $"Pattern '{pattern}' contains an invalid character '{token[j]}' in token '{token}'.",
                        nameof(pattern)
                    );
        }

        return new SubjectPattern(tokens, hasMultiWildcard);
    }

    /// <summary>
    /// Returns whether the given concrete subject matches this pattern.
    /// </summary>
    /// <param name="subject">The concrete subject to test.</param>
    /// <returns>True if the subject matches; otherwise false.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="subject"/> is not a valid concrete subject.</exception>
    public bool Matches(string subject)
    {
        Subject.Validate(subject);

        // Allocation-free: walk the subject's tokens via span slices (no Split). The trailing ">" (when
        // HasMultiWildcard) absorbs one or more tokens, so only the first fixedCount tokens are compared.
        var fixedCount = HasMultiWildcard ? Tokens.Count - 1 : Tokens.Count;
        var span = subject.AsSpan();

        var index = 0;
        var pos = 0;
        while (true)
        {
            var dot = span[pos..].IndexOf('.');
            var end = dot < 0 ? span.Length : pos + dot;
            var token = span[pos..end];

            if (index < fixedCount)
            {
                if (!TokenMatches(Tokens[index], token))
                    return false;
            }
            else if (!HasMultiWildcard)
            {
                // more subject tokens than a non-wildcard pattern → no match
                return false;
            }

            index++;
            if (dot < 0)
                break;
            pos = end + 1;
        }

        // ">" requires at least one trailing token beyond the fixed prefix; otherwise counts must be equal.
        return HasMultiWildcard ? index >= fixedCount + 1 : index == fixedCount;
    }

    /// <summary>
    /// Matches a single pattern token against a single subject token.
    /// </summary>
    /// <param name="patternToken">The pattern token (literal or <c>*</c>).</param>
    /// <param name="subjectToken">The concrete subject token.</param>
    /// <returns>True if the tokens match.</returns>
    private static bool TokenMatches(string patternToken, ReadOnlySpan<char> subjectToken) =>
        patternToken == SingleWildcard || subjectToken.SequenceEqual(patternToken);
}
