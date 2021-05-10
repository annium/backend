using System;

namespace Annium.Core.Primitives
{
    public static class RandomExtensions
    {
        public static decimal NextDecimal(this Random random) => (decimal) random.NextDouble();
    }
}