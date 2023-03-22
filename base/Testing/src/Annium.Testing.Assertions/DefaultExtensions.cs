using System.Reflection;
using System.Runtime.CompilerServices;
using Annium.Core.Mapper;
using Annium.Data.Models.Extensions;
using Annium.Testing.Assertions.Internal;

// ReSharper disable once CheckNamespace
namespace Annium.Testing;

public static class ValueExtensions
{
    public static T IsDefault<T>(
        this T value,
        string? message = null,
        [CallerArgumentExpression("value")] string valueEx = default!
    )
    {
        value.Is(default, $"{value.Wrap(valueEx)} is not default");

        return value;
    }

    public static T IsNotDefault<T>(
        this T value,
        string? message = null,
        [CallerArgumentExpression("value")] string valueEx = default!
    )
    {
        value.IsNot(default, $"{value.Wrap(valueEx)} is default");

        return value;
    }
}