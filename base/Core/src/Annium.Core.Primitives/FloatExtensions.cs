using System;
using System.Runtime.CompilerServices;

namespace Annium.Core.Primitives;

public static class FloatExtensions
{
    public static float DiffFrom(this float value, float from) =>
        from == 0f ? float.PositiveInfinity : Math.Abs((value - from) / from);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAround(this float value, float to, float precision) =>
        value.DiffFrom(to) <= precision;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Within(this float value, float min, float max) => value.Above(min).Below(max);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Above(this float value, float min) => Math.Max(value, min);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Below(this float value, float max) => Math.Min(value, max);
}