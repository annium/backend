using System.Reflection;
using System.Runtime.CompilerServices;
using Annium.Core.Mapper;
using Annium.Data.Models.Extensions;
using Annium.Testing.Assertions.Internal;

namespace Annium.Testing;

public static class ValueExtensions
{
    public static T IsDefault<T>(
        this T value,
        string? message = null,
        IMapper? mapper = default,
        [CallerArgumentExpression("value")] string valueEx = default!
    )
    {
        if (!value.IsShallowEqual(default(T)!, mapper ?? Mapper.GetFor(Assembly.GetCallingAssembly())))
            throw new AssertionFailedException(message ?? $"{value.Wrap(valueEx)} is not default");

        return value;
    }

    public static T IsNotDefault<T>(
        this T value,
        string? message = null,
        IMapper? mapper = default,
        [CallerArgumentExpression("value")] string valueEx = default!
    )
    {
        if (value.IsShallowEqual(default(T)!, mapper ?? Mapper.GetFor(Assembly.GetCallingAssembly())))
            throw new AssertionFailedException(message ?? $"{value.Wrap(valueEx)} is default");

        return value;
    }
}