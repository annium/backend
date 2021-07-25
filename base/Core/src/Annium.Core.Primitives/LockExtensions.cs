using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Annium.Core.Primitives
{
    public static class LockExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LockedWith<TL>(this Action get, TL locker) where TL : class
        {
            lock (locker) get();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T LockedWith<TL, T>(this Func<T> get, TL locker) where TL : class
        {
            lock (locker) return get();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IDisposable Lock<T>(this T subject) where T : class
        {
            Monitor.Enter(subject);

            return Disposable.Create(() => Monitor.Exit(subject));
        }
    }
}