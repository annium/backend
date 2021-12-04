using System;

namespace Annium.Core.Primitives;

public static class FloatExtensions
{
    public static float DiffFrom(this float value, float from) =>
        from == 0f ? float.PositiveInfinity : Math.Abs((value - from) / from);

    public static bool IsAround(this float value, float to, float precision) =>
        value.DiffFrom(to) <= precision;
}