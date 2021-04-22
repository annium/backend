using System.Runtime.CompilerServices;
using System.Threading;

namespace Annium.Diagnostics.Debug
{
    public static class IdExtensions
    {
        public static string GetId<T>(this T obj) where T : class => IdTable<T>.GetId(obj);

        private class IdTable<T>
            where T : class
        {
            private static long Id;
            private static readonly ConditionalWeakTable<T, string> Ids = new();

            public static string GetId(T obj) =>
                Ids.GetValue(obj, _ => Interlocked.Increment(ref Id).ToString("000"));
        }
    }
}