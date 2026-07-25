using System;
using System.Diagnostics.CodeAnalysis;

namespace Annium.MessageBus.Abstractions;

/// <summary>
/// Helpers for the canonical subject format: dot-separated tokens, each non-empty and made of
/// <c>[A-Za-z0-9_-]</c>. Wildcards (<c>*</c>, <c>&gt;</c>) are not valid in a concrete subject — see
/// <see cref="SubjectPattern"/> for subscription patterns.
/// </summary>
public static class Subject
{
    /// <summary>
    /// Validates a concrete subject, throwing if it is malformed.
    /// </summary>
    /// <param name="subject">The subject to validate.</param>
    /// <exception cref="ArgumentException">Thrown when the subject is null, empty, or malformed.</exception>
    public static void Validate(string subject)
    {
        if (TryGetError(subject, out var error))
            throw new ArgumentException(error, nameof(subject));
    }

    /// <summary>
    /// Returns whether the given string is a valid concrete subject.
    /// </summary>
    /// <param name="subject">The subject to check.</param>
    /// <returns>True if valid; otherwise false.</returns>
    public static bool IsValid(string subject) => !TryGetError(subject, out _);

    /// <summary>
    /// Resolves the subject declared by a subject-aware message type.
    /// </summary>
    /// <typeparam name="T">The subject-aware message type.</typeparam>
    /// <returns>The type's canonical subject.</returns>
    public static string Of<T>()
        where T : ISubjectAware => T.Subject;

    /// <summary>
    /// Returns whether the character is allowed inside a subject token.
    /// </summary>
    /// <param name="c">The character to check.</param>
    /// <returns>True if the character is a letter, digit, '-' or '_'.</returns>
    internal static bool IsValidTokenChar(char c) => char.IsAsciiLetterOrDigit(c) || c == '-' || c == '_';

    /// <summary>
    /// Single-pass, allocation-free validation core: scans the subject character by character (no
    /// <c>Split</c>), reporting the first structural error. Returns true (with a message) when malformed.
    /// </summary>
    /// <param name="subject">The subject to inspect.</param>
    /// <param name="error">The error message when malformed; otherwise null.</param>
    /// <returns>True if an error was found; otherwise false.</returns>
    private static bool TryGetError(string subject, [NotNullWhen(true)] out string? error)
    {
        if (string.IsNullOrEmpty(subject))
        {
            error = "Subject must be a non-empty string.";
            return true;
        }

        var tokenLength = 0;
        for (var i = 0; i < subject.Length; i++)
        {
            var c = subject[i];
            if (c == '.')
            {
                if (tokenLength == 0)
                {
                    error = $"Subject '{subject}' has an empty token (no leading/trailing/double dots).";
                    return true;
                }

                tokenLength = 0;
            }
            else if (IsValidTokenChar(c))
            {
                tokenLength++;
            }
            else
            {
                error = $"Subject '{subject}' contains an invalid character '{c}'. Allowed: letters, digits, '-', '_'.";
                return true;
            }
        }

        if (tokenLength == 0)
        {
            error = $"Subject '{subject}' has an empty token (no leading/trailing/double dots).";
            return true;
        }

        error = null;
        return false;
    }
}
