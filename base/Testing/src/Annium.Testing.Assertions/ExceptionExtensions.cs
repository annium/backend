using System;
using System.Reflection;
using Annium.Core.Mapper;

namespace Annium.Testing
{
    public static class ExceptionExtensions
    {
        public static T Reports<T>(this T value, string message) where T : Exception
        {
            value.Message.IsEqual(
                message,
                Mapper.GetFor(Assembly.GetCallingAssembly()),
                $"Expected exception message `{message}`, got `{value.Message}`"
            );

            return value;
        }
    }
}