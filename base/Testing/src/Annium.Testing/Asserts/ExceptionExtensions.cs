using System;

namespace Annium.Testing
{
    public static class ExceptionExtensions
    {
        public static T Reports<T>(this T value, string message) where T : Exception
        {
            value.Message.IsEqual(message, $"Expected exception message `{message}`, got `{value.Message}`");

            return value;
        }
    }
}