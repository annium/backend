using System;

namespace Annium.Core.Primitives
{
    public static class DecimalExtensions
    {
        public static decimal DiffFrom(this decimal value, decimal from) =>
            from == 0m ? decimal.MaxValue : Math.Abs((value - from) / from);

        public static bool IsAround(this decimal value, decimal to, decimal precision) =>
            value.DiffFrom(to) <= precision;
    }
}