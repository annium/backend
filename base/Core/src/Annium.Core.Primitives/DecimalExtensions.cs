using System;
using System.Runtime.CompilerServices;

namespace Annium.Core.Primitives;

public static class DecimalExtensions
{
    public static decimal DiffFrom(this decimal value, decimal from) =>
        from == 0m ? decimal.MaxValue : value.DiffFromInternal(from);

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
    public static int CeilInt32(this decimal value) =>
        (int)Math.Ceiling(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long CeilInt64(this decimal value) =>
        (long)Math.Ceiling(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int RoundInt32(this decimal value) =>
        (int)Math.Round(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int RoundInt32(this decimal value, int digits) =>
        (int)Math.Round(value, digits);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int RoundInt32(this decimal value, MidpointRounding mode) =>
        (int)Math.Round(value, mode);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int RoundInt32(this decimal value, int digits, MidpointRounding mode) =>
        (int)Math.Round(value, digits, mode);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long RoundInt64(this decimal value) =>
        (long)Math.Round(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long RoundInt64(this decimal value, int digits) =>
        (long)Math.Round(value, digits);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long RoundInt64(this decimal value, MidpointRounding mode) =>
        (long)Math.Round(value, mode);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long RoundInt64(this decimal value, int digits, MidpointRounding mode) =>
        (long)Math.Round(value, digits, mode);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static decimal Within(this decimal value, decimal min, decimal max) => value.Above(min).Below(max);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static decimal Above(this decimal value, decimal min) => Math.Max(value, min);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static decimal Below(this decimal value, decimal max) => Math.Min(value, max);

    public static decimal ToPretty(this decimal value, decimal diff)
    {
        if (value == 0m)
            return 0;

        var result = value;

        var decimals = value.Decimals();
        if (decimals > 0)
        {
            while (decimals > 0)
            {
                var next = Math.Round(result, --decimals);
                if (next.DiffFromInternal(value) > diff)
                    return result;

                result = next;
            }
        }

        var mul = 10;
        while (true)
        {
            var next = Math.Round(result / mul) * mul;
            if (next.DiffFromInternal(value) > diff)
                return result;

            result = next;
            mul *= 10;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static decimal DiffFromInternal(this decimal value, decimal from) => Math.Abs((value - from) / from);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int Decimals(this decimal value) => decimal.GetBits(value)[3] >> 16;
}