using System;
using System.Runtime.CompilerServices;

namespace Annium.Diagnostics.Debug
{
    public static class IdExtensions
    {
        private static ConditionalWeakTable<object, string> _ids = new ConditionalWeakTable<object, string>();

        public static string GetId<T>(this T obj) where T : class =>
            _ids.GetValue(obj, e => Guid.NewGuid().ToString());

        public static void Trace<T>(this T obj, string message, bool withTrace = false) where T : class =>
            Console.WriteLine(
                $"{obj.GetType().Name}[{obj.GetId()}]{message}" +
                (withTrace ? $"{Environment.NewLine}{Environment.StackTrace}" : string.Empty)
            );
    }
}