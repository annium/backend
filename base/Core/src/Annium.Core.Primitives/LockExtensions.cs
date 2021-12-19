using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Annium.Core.Primitives;

public static class LockExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IDisposable Lock<T>(this T subject) where T : class
    {
        Monitor.Enter(subject);

        return Disposable.Create(() => Monitor.Exit(subject));
    }
}