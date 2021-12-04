using System;
using System.Linq;
using Annium.Core.Primitives.Collections.Generic;

namespace Annium.Testing;

public static class ExceptionExtensions
{
    public static T Reports<T>(this T value, string message, params string[] messages)
        where T : Exception
    {
        var allMessages = message.Yield().Concat(messages).ToArray();
        if (!allMessages.All(value.Message.Contains))
            throw new AssertionFailedException(
                $"Expected exception message `{value.Message}` to contain: `{allMessages.Join("`, `")}`"
            );

        return value;
    }

    public static T ReportsExactly<T>(this T value, string message)
        where T : Exception
    {
        value.Message.Is(
            message,
            $"Expected exception message `{value.Message}` to be `{message}`"
        );

        return value;
    }
}