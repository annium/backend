using System;
using System.Runtime.CompilerServices;

namespace Annium.Core.Primitives;

public static class DoubleExtensions
{
    public static double DiffFrom(this double value, double from) =>
        from == 0d ? double.PositiveInfinity : Math.Abs((value - from) / from);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAround(this double value, double to, double precision) =>
        value.DiffFrom(to) <= precision;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int FloorInt32(this double value) =>
        (int)Math.Floor(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long FloorInt64(this double value) =>
        (long)Math.Floor(value);
}