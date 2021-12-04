using System.Reflection;
using System.Text.Json;
using Annium.Core.Mapper;
using Annium.Data.Models.Extensions;

namespace Annium.Testing;

public static class ShallowEqualityExtensions
{
    public static void IsEqual<T, TD>(this T value, TD data, string message = "", IMapper? mapper = default)
    {
        if (!value.IsShallowEqual(data, mapper ?? Mapper.GetFor(Assembly.GetCallingAssembly())))
            throw new AssertionFailedException(
                string.IsNullOrEmpty(message)
                    ? $"{JsonSerializer.Serialize(value)} is not equal to {JsonSerializer.Serialize(data)}"
                    : message
            );
    }

    public static void IsNotEqual<T, TD>(this T value, TD data, string message = "", IMapper? mapper = default)
    {
        if (value.IsShallowEqual(data, mapper ?? Mapper.GetFor(Assembly.GetCallingAssembly())))
            throw new AssertionFailedException(
                string.IsNullOrEmpty(message)
                    ? $"{JsonSerializer.Serialize(value)} is equal to {JsonSerializer.Serialize(data)}"
                    : message
            );
    }

    public static void IsEqual<T, TD>(this T value, TD data, IMapper mapper, string message)
    {
        if (!value.IsShallowEqual(data, mapper))
            throw new AssertionFailedException(message);
    }

    public static void IsNotEqual<T, TD>(this T value, TD data, IMapper mapper, string message)
    {
        if (value.IsShallowEqual(data, mapper))
            throw new AssertionFailedException(message);
    }
}