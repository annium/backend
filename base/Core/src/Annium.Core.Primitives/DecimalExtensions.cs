using System;
using System.Runtime.CompilerServices;

namespace Annium.Core.Primitives;

public static class DecimalExtensions
{
    public static decimal DiffFrom(this decimal value, decimal from) =>
        from == 0m ? decimal.MaxValue : Math.Abs((value - from) / from);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAround(this decimal value, decimal to, decimal precision) =>
        value.DiffFrom(to) <= precision;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int FloorInt32(this decimal value) =>
        (int)Math.Floor(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long FloorInt64(this decimal value) =>
        (long)Math.Floor(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static decimal Within(this decimal value, decimal min, decimal max) => value.Above(min).Below(max);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static decimal Above(this decimal value, decimal min) => Math.Max(value, min);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static decimal Below(this decimal value, decimal max) => Math.Min(value, max);
}