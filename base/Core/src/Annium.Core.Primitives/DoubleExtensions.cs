using System;

namespace Annium.Core.Primitives
{
    public static class DoubleExtensions
    {
        public static double DiffFrom(this double value, double from) =>
            from == 0d ? double.PositiveInfinity : Math.Abs((value - from) / from);

        public static bool IsAround(this double value, double to, double precision) =>
            value.DiffFrom(to) <= precision;
    }
}