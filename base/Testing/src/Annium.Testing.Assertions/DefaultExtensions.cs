using System.Reflection;
using System.Text.Json;
using Annium.Core.Mapper;
using Annium.Data.Models.Extensions;

namespace Annium.Testing
{
    public static class ValueExtensions
    {
        public static T IsDefault<T>(this T value, string message = "", IMapper? mapper = default)
        {
            if (!value.IsShallowEqual(default(T)!, mapper ?? Mapper.GetFor(Assembly.GetCallingAssembly())))
                throw new AssertionFailedException(
                    string.IsNullOrEmpty(message)
                        ? $"{JsonSerializer.Serialize(value)} is not default"
                        : message
                );

            return value;
        }

        public static T IsNotDefault<T>(this T value, string message = "", IMapper? mapper = default)
        {
            if (value.IsShallowEqual(default(T)!, mapper ?? Mapper.GetFor(Assembly.GetCallingAssembly())))
                throw new AssertionFailedException(
                    string.IsNullOrEmpty(message)
                        ? $"{JsonSerializer.Serialize(value)} is default"
                        : message
                );

            return value;
        }
    }
}