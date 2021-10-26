using Annium.Diagnostics.Debug;

namespace Annium.Core.Primitives
{
    public static class IdExtensions
    {
        public static string GetFullId<T>(this T obj) where T : class =>
            obj is null! ? "null" : $"{obj.GetType().FriendlyName()}#{obj.GetId()}";
    }
}