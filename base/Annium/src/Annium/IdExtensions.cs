using Annium.Diagnostics.Debug;

namespace Annium;

public static class IdExtensions
{
    public static string GetFullId<T>(this T obj) =>
        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        obj is null ? "null" : $"{obj.GetType().FriendlyName()}#{obj.GetId()}";
}