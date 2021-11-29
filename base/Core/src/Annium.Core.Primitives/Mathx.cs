using System;

namespace Annium.Core.Primitives
{
    public static class Mathx
    {
        public static float RelativeDiff(float value, float from) =>
            from == 0f ? float.PositiveInfinity : Math.Abs((value - from) / from);

        public static double RelativeDiff(double value, double from) =>
            from == 0d ? double.PositiveInfinity : Math.Abs((value - from) / from);

        public static decimal RelativeDiff(decimal value, decimal from) =>
            from == 0m ? decimal.MaxValue : Math.Abs((value - from) / from);
    }
}