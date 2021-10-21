using System;

namespace Annium.Core.Primitives
{
    public static class Mathx
    {
        public static float RelativeDiff(float value, float from) =>
            Math.Abs(value - from) / from;

        public static double RelativeDiff(double value, double from) =>
            Math.Abs(value - from) / from;

        public static decimal RelativeDiff(decimal value, decimal from) =>
            Math.Abs(value - from) / from;
    }
}