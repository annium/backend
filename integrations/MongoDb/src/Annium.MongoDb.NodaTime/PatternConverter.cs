using NodaTime.Text;

namespace Annium.MongoDb.NodaTime;

/// <summary>
/// Extension methods for NodaTime patterns to support checked parsing
/// </summary>
internal static class PatternExtensions
{
    /// <summary>
    /// Parses a string using the pattern and throws an exception if parsing fails
    /// </summary>
    /// <typeparam name="TResult">The type of value to parse</typeparam>
    /// <param name="pattern">The pattern to use for parsing</param>
    /// <param name="candidate">The string to parse</param>
    /// <returns>The parsed value</returns>
    /// <exception cref="System.Exception">Thrown when parsing fails</exception>
    public static TResult CheckedParse<TResult>(this IPattern<TResult> pattern, string candidate)
    {
        var value = pattern.Parse(candidate);
        if (!value.Success)
            throw value.Exception;

        return value.Value;
    }
}
